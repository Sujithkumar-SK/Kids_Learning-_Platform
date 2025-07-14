using System.ComponentModel.DataAnnotations;

namespace KidsLearningPlatform.Models
{
  public class User
{
  public int Id { get; set; }

  [Required(ErrorMessage = "Name is Reqiured")]
  public string Name { get; set; }

  [Range(1,90, ErrorMessage ="Age must be greater than 0")]
  public int Age { get; set; }

  [Required(ErrorMessage = "Email is required")]
  [EmailAddress(ErrorMessage ="Invalid email address")]
  public string Email{ get; set; }

  [Required(ErrorMessage ="Username is required")]
  public string Username{ get; set; }

  [Required(ErrorMessage = "Password is required")]
  [DataType(DataType.Password)]
  public string Password{ get; set; }

  [Required(ErrorMessage = "User type is required")]
  public string UserType{ get; set; }
}
}

