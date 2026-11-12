using System.ComponentModel.DataAnnotations;
using Application.DTOs.Enrollments;

namespace Application.DTOs.Students;

public class StudentFilterDto
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 8;
}

public class StudentDetailDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<EnrollmentDto> Enrollments { get; set; } = [];
}

public class UpdateStudentDto
{
    [Required(ErrorMessage = "Имя обязательно")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Имя должно содержать от 2 до 50 символов")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Фамилия обязательна")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Фамилия должна содержать от 2 до 50 символов")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email обязателен")]
    [EmailAddress(ErrorMessage = "Введите корректный Email")]
    [StringLength(100, ErrorMessage = "Email не должен превышать 100 символов")]
    public string Email { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

public class UpdateStudentStatusDto
{
    [Required]
    public bool IsActive { get; set; }
}
