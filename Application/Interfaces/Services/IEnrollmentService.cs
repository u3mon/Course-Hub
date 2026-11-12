using Application.Common;
using Application.DTOs.Enrollments;

namespace Application.Interfaces.Services;

public interface IEnrollmentService
{
    Task<Result<EnrollmentDto>> EnrollStudentAsync(
        int? userId,
        int courseId,
        bool isAdminOverride = false,
        int? adminId = null,
        string? adminEmail = null,
        CancellationToken cancellationToken = default);

    Task<Result> UnenrollStudentAsync(
        int? userId,
        int courseId,
        int? adminId = null,
        string? adminEmail = null,
        CancellationToken cancellationToken = default);

    Task<Result<List<EnrollmentDto>>> GetStudentCoursesAsync(int? userId, CancellationToken cancellationToken = default);
    Task<Result<List<EnrollmentDto>>> GetCourseStudentsAsync(int courseId, CancellationToken cancellationToken = default);
}
