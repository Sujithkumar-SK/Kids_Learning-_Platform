using System.ComponentModel;
using System.Reflection;
using KidsLearningPlatform.Models;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace KidsLearningPlatform.Controllers
{
  public class AdminController : Controller
  {
    private readonly IConfiguration? _config;
    public AdminController(IConfiguration config)
    {
      _config = config;
    }
    public IActionResult Index()
    {
      if (HttpContext.Session.GetString("UserType") != "Admin")
      {
        return RedirectToAction("Login", "User");
      }
      List<Course> courses = new();
      using SqlConnection conn = new(_config!.GetConnectionString("DefaultConnection"));
      string query = @"select * from courses";
      using SqlCommand cmd = new SqlCommand(query, conn);
      conn.Open();
      using SqlDataReader reader = cmd.ExecuteReader();
      while (reader.Read())
      {
        courses.Add(new Course
        {
          Id = Convert.ToInt32(reader["id"]),
          Title = reader["Title"].ToString(),
          Description = reader["Description"].ToString(),
          Duration = Convert.ToInt32(reader["Duration"]),
          Instructor = reader["Instructor"].ToString(),
          CourseImage = reader["CourseImage"] as byte[]
        });
      }
      return View(courses);
    }

    public IActionResult Create()
    {
      return View();
    }
    [HttpPost]
    public IActionResult Create(Course course)
    {
      if (!ModelState.IsValid)
      {
        return View(course);
      }
      byte[]? imageData = null;
      if (course.ImageFile != null && course.ImageFile.Length > 0)
      {
        using var ms = new MemoryStream();
        course.ImageFile.CopyTo(ms);
        imageData = ms.ToArray();
      }
      using SqlConnection conn = new(_config?.GetConnectionString("DefaultConnection"));
      string query = "sp_InsertCourse";
      using SqlCommand cmd = new(query, conn);
      cmd.CommandType = System.Data.CommandType.StoredProcedure;
      cmd.Parameters.AddWithValue("@Title", course.Title);
      cmd.Parameters.AddWithValue("@Description", course.Description ?? (object)DBNull.Value);
      cmd.Parameters.AddWithValue("@Duration", course.Duration);
      cmd.Parameters.AddWithValue("@Instructor", course.Instructor ?? (object)DBNull.Value);
      cmd.Parameters.AddWithValue("@CourseImage", imageData ?? (object)DBNull.Value);

      conn.Open();
      cmd.ExecuteNonQuery();
      conn.Close();
      TempData["Message"] = "Course Added successfully";
      return RedirectToAction("Index");
    }

    public IActionResult Edit(int id)
    {
      Course course = new();
      using SqlConnection conn = new(_config?.GetConnectionString("DefaultConnection"));
      string query = "SELECT * FROM Courses WHERE Id = @Id";
      using SqlCommand cmd = new(query, conn);
      cmd.Parameters.AddWithValue("@Id", id);
      conn.Open();
      using SqlDataReader reader = cmd.ExecuteReader();
      if (reader.Read())
      {
        course.Id = Convert.ToInt32(reader["Id"]);
        course.Title = reader["Title"].ToString();
        course.Description = reader["Description"].ToString();
        course.Duration = Convert.ToInt32(reader["Duration"]);
        course.Instructor = reader["Instructor"].ToString();
        course.CourseImage = reader["CourseImage"] as byte[];
      }
      return View(course);
    }
    [HttpPost]
    public IActionResult Edit(Course course)
    {
      if (!ModelState.IsValid)
      {
        return View(course);
      }
      byte[]? imageData = null;
      if (course.ImageFile != null && course.ImageFile.Length > 0)
      {
        using var ms = new MemoryStream();
        course.ImageFile.CopyTo(ms);
        imageData = ms.ToArray();
      }
      else
      {
        imageData = course.CourseImage;
      }
      using SqlConnection conn = new(_config?.GetConnectionString("DefaultConnection"));
      string query = "sp_UpdateCourse";
      using SqlCommand cmd = new(query, conn);
      cmd.CommandType = System.Data.CommandType.StoredProcedure;
      cmd.Parameters.AddWithValue("@Title", course.Title);
      cmd.Parameters.AddWithValue("@Description", course.Description ?? (object)DBNull.Value);
      cmd.Parameters.AddWithValue("@Duration", course.Duration);
      cmd.Parameters.AddWithValue("@Instructor", course.Instructor ?? (object)DBNull.Value);
      cmd.Parameters.Add("@CourseImage", System.Data.SqlDbType.VarBinary).Value = imageData ?? (object)DBNull.Value;
      cmd.Parameters.AddWithValue("@Id", course.Id);
      conn.Open();
      cmd.ExecuteNonQuery();
      conn.Close();
      TempData["Message"] = "Course updated Successfully!..";
      return RedirectToAction("Index");
    }
    public IActionResult Delete(int id)
    {
      using SqlConnection conn = new(_config?.GetConnectionString("DefaultConnection"));
      string query = "DELETE FROM Courses WHERE Id = @Id";
      using SqlCommand cmd = new(query, conn);
      cmd.Parameters.AddWithValue("@Id", id);
      conn.Open();
      cmd.ExecuteNonQuery();
      conn.Close();

      TempData["Message"] = "Course deleted successfully!";
      return RedirectToAction("Index");
    }
    public IActionResult GetImage(int id)
    {
      byte[]? image = null;
      using SqlConnection conn = new(_config?.GetConnectionString("DefaultConnection"));
      string query = "SELECT CourseImage FROM Courses WHERE Id = @Id";
      using SqlCommand cmd = new(query, conn);
      cmd.Parameters.AddWithValue("@Id", id);
      conn.Open();
      using SqlDataReader reader = cmd.ExecuteReader();
      if (reader.Read() && reader["CourseImage"] != DBNull.Value)
      {
        image = (byte[])reader["CourseImage"];
      }

      if (image == null)
        return NotFound();

      return File(image, "image/jpeg");
    }
  }
}