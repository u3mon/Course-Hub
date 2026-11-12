using Application.DTOs.Enrollments;

namespace Application.DTOs.Admin;

public class AdminActionLogDto
{
    public int Id { get; set; }
    public int AdminId { get; set; }
    public string AdminEmail { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public string Details { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class AdminDashboardDto
{
    public int TotalStudents { get; set; }
    public int ActiveStudents { get; set; }
    public int BlockedStudents { get; set; }
    public int TotalCourses { get; set; }
    public int AvailableCourses { get; set; }
    public int TotalCategories { get; set; }
    public int TotalEnrollments { get; set; }
    public List<EnrollmentDto> RecentEnrollments { get; set; } = [];
    public List<AdminActionLogDto> RecentLogs { get; set; } = [];
}
