using System.ComponentModel.DataAnnotations;

namespace KidsLearningPlatform.Models
{
  public class User
  {

    private string? _name;
    private string? _email;
    private string? _username;
    private string? _password;
    private string? _usertype;
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is Reqiured")]
    public string Name
    {
      get => _name!;
      set => _name = value?.Trim()!;
    }

    [Range(1, 90, ErrorMessage = "Age must be greater than 0")]
    public int Age { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email
    {
      get => _email!;
      set => _email = value?.Trim()!;
    }

    [Required(ErrorMessage = "Username is required")]
    public string Username
    {
      get => _username!;
      set => _username = value?.Trim()!;
    }

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string Password
    {
      get => _password!;
      set => _password = value?.Trim()!;
    }

    [Required(ErrorMessage = "User type is required")]
    public string UserType
    {
      get => _usertype!;
      set => _usertype = value?.Trim()!;
    }

    public byte[]? ProfileImage { get; set; }
  }
}

