namespace Application.DTOs.Enrollments;

public class EnrollmentDto
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string InstructorName { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsCourseAvailable { get; set; }
    public int UserId { get; set; }
    public string StudentFirstName { get; set; } = string.Empty;
    public string StudentLastName { get; set; } = string.Empty;
    public string StudentFullName => $"{StudentFirstName} {StudentLastName}".Trim();
    public string StudentEmail { get; set; } = string.Empty;
    public bool StudentIsActive { get; set; }
    public DateTime EnrollDate { get; set; }
}
