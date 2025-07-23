using KidsLearningPlatform.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;

namespace KidsLearningPlatform.Controllers
{
  public class UserController : Controller
  {
    private readonly IConfiguration _config;

    public UserController(IConfiguration config)
    {
      _config = config;
    }

    public IActionResult Signup()
    {
      return View();
    }

    [HttpPost]
    public IActionResult Signup(User user)
    {
      if (!ModelState.IsValid)
      {
        return View(user);
      }
      using SqlConnection conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
      string query = "INSERT INTO Users (Name, Age, Email, Username, Password, UserType) VALUES (@Name, @Age, @Email, @Username, @Password, @UserType)";
      using SqlCommand cmd = new SqlCommand(query, conn);
      cmd.Parameters.AddWithValue("@Name", user.Name);
      cmd.Parameters.AddWithValue("@Age", user.Age);
      cmd.Parameters.AddWithValue("@Email", user.Email);
      cmd.Parameters.AddWithValue("@Username", user.Username);
      cmd.Parameters.AddWithValue("@Password", PasswordHelper.HashPassword(user.Password));
      cmd.Parameters.AddWithValue("@UserType", user.UserType);

      conn.Open();
      int result = cmd.ExecuteNonQuery();
      conn.Close();
      if (result > 0)
      {
        TempData["Message"] = "Signup Successful! please login.";
        return RedirectToAction("Login");
      }
      else
      {
        ViewBag.Message = "Signup failed!";
        return View();
      }
    }
    public IActionResult Login()
    {
      return View();
    }
    [HttpPost]
    public IActionResult Login(LoginModel login)
    {
      if (!ModelState.IsValid)
      {
        return View(login);
      }
      string hashedPassword = PasswordHelper.HashPassword(login.Password);
      using SqlConnection conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
      string query = "SELECT * FROM Users WHERE Username = @Username AND Password = @Password";
      using SqlCommand cmd = new SqlCommand(query, conn);
      cmd.Parameters.AddWithValue("@Username", login.Username);
      cmd.Parameters.AddWithValue("@Password", hashedPassword);
      conn.Open();

      using SqlDataReader reader = cmd.ExecuteReader();

      if (reader.Read())
      {
        HttpContext.Session.SetString("Username", login.Username);
        HttpContext.Session.SetString("UserId", reader["Id"].ToString()!);
        HttpContext.Session.SetString("UserType", reader["UserType"].ToString()!);
        conn.Close();
        return RedirectToAction("Dashboard");
      }
      else
      {
        conn.Close();
        ViewBag.Message = "Invalid username or password!";
        return View(login);
      }
    }

    public IActionResult Dashboard(string Keyword, string instructor)
    {
      if (HttpContext.Session.GetString("UserId") == null)
      {
        return RedirectToAction("Login");
      }
      ViewBag.Username = HttpContext.Session.GetString("Username");
      ViewBag.UserId = HttpContext.Session.GetString("UserId");
      List<Course> courses = new();
      HashSet<string> InstructorsSet = new HashSet<string>();
      using SqlConnection conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
      string query = "select * from courses Where 1=1";
      if (!string.IsNullOrEmpty(Keyword))
      {
        query+="and (Title like @Keyword or Description like @Keyword)";
      }
      if(!string.IsNullOrEmpty(instructor))
      {
        query += "and Instructor = @Instructor";
      }
      using SqlCommand cmd = new SqlCommand(query, conn);
      conn.Open();
      if (!string.IsNullOrEmpty(Keyword))
      {
        cmd.Parameters.AddWithValue("@Keyword", "%"+Keyword+"%");
      }
      if (!string.IsNullOrEmpty(instructor))
      {
        cmd.Parameters.AddWithValue("@Instructor",instructor);
      }
      using SqlDataReader reader = cmd.ExecuteReader();
      while (reader.Read())
      {
        courses.Add(new Course
        {
          Id = Convert.ToInt32(reader["Id"]),
          Title = reader["Title"].ToString(),
          Description = reader["Description"].ToString(),
          Duration = Convert.ToInt32(reader["Duration"]),
          Instructor = reader["Instructor"].ToString(),
          CourseImage = reader["CourseImage"] as byte[]
        });
        InstructorsSet.Add(reader["Instructor"].ToString()!);
      }
      conn.Close();
      ViewBag.SearchKeyword = Keyword;
      ViewBag.selectedInstructor = instructor;
      List<string> instructorsList = new List<string>(InstructorsSet);
      instructorsList.Sort();
      ViewBag.Instructors = instructorsList;
      return View(courses);
    }

    public IActionResult Logout()
    {
      HttpContext.Session.Clear();
      TempData["Message"] = "Logged out Successfully";
      return RedirectToAction("Login");
    }
    public IActionResult EditProfile()
    {
      if (HttpContext.Session.GetString("Username") == null)
      {
        return RedirectToAction("Login");
      }
      string username = HttpContext.Session.GetString("Username")!;
      User user = new User();
      using SqlConnection conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
      string query = @"select * from users where Username=@Username";
      using SqlCommand cmd = new SqlCommand(query, conn);
      cmd.Parameters.AddWithValue("@Username", username);
      conn.Open();
      using SqlDataReader reader = cmd.ExecuteReader();
      if (reader.Read())
      {
        user.Id = Convert.ToInt32(reader["Id"]);
        user.Name = reader["Name"].ToString()!;
        user.Age = Convert.ToInt32(reader["Age"]);
        user.Email = reader["Email"].ToString()!;
        user.UserType = reader["UserType"].ToString()!;
        user.Username = reader["Username"].ToString()!;
      }
      conn.Close();
      return View(user);
    }

    [HttpPost]
    public IActionResult EditProfile(User updatedUser, IFormFile profilePhoto)
    {
      if (HttpContext.Session.GetString("Username") == null)
      {
        return RedirectToAction("Login");
      }
      string sessionUsername = HttpContext.Session.GetString("Username")!;
      using SqlConnection conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
      string query = @"UPDATE Users 
                      SET Name = @Name, Age = @Age, Email = @Email, UserType = @UserType, ProfileImage = @ProfileImage
                      WHERE Username = @Username";
      using SqlCommand cmd = new SqlCommand(query, conn);
      cmd.Parameters.AddWithValue("@Name", updatedUser.Name);
      cmd.Parameters.AddWithValue("@Age", updatedUser.Age);
      cmd.Parameters.AddWithValue("@Email", updatedUser.Email);
      cmd.Parameters.AddWithValue("@UserType", updatedUser.UserType);
      cmd.Parameters.AddWithValue("@Username", sessionUsername);
      byte[]? imageData = null;
      if (profilePhoto != null && profilePhoto.Length > 0)
      {
        using var ms = new MemoryStream();
        profilePhoto.CopyTo(ms);
        imageData = ms.ToArray();
        cmd.Parameters.Add("@ProfileImage", System.Data.SqlDbType.VarBinary).Value = imageData;
      }
      else
      {
        cmd.Parameters.Add("@ProfileImage", System.Data.SqlDbType.VarBinary).Value = DBNull.Value;
      }
      conn.Open();
      int result = cmd.ExecuteNonQuery();
      conn.Close();

      if (result > 0)
      {
        TempData["Message"] = "Profile updated Successfully!";
        return RedirectToAction("Dashboard");
      }
      else
      {
        ViewBag.Message = "Update Failed!";
        return View(updatedUser);
      }
    }

    public IActionResult GetProfileImage(int id)
    {
      byte[]? imageData = null;
      using SqlConnection conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
      string query = "SELECT ProfileImage FROM Users WHERE Id = @Id";
      using SqlCommand cmd = new SqlCommand(query, conn);
      cmd.Parameters.AddWithValue("@Id", id);
      conn.Open();
      using SqlDataReader reader = cmd.ExecuteReader();
      if (reader.Read() && reader["ProfileImage"] != DBNull.Value)
      {
        imageData = (byte[])reader["ProfileImage"];
      }
      if (imageData == null)
      {
        return NotFound();
      }
      return File(imageData, "image/jpeg");
    }
  }
}