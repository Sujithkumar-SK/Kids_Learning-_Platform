using System;
using System.ComponentModel.DataAnnotations;

namespace KidsLearningPlatform.Models
{
  public class Course
  {
    public int Id { get; set; }

    [Required]
    public string? Title { get; set; }

    [Required]
    public string? Description { get; set; }

    [Required]
    public int Duration { get; set; }

    [Required]
    public string? Instructor { get; set; }

    public byte[]? CourseImage { get; set; }

    public IFormFile? ImageFile { get; set; }

  }
}