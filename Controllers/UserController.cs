using KidsLearningPlatform.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

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

    public IActionResult Dashboard()
    {
      return View();
    }
  }
}