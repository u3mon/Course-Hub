using Application.Common;
using Application.DTOs.Admin;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;

namespace Application.Services;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly IUserRepository _userRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IAdminActionLogRepository _logRepository;

    public AdminDashboardService(
        IUserRepository userRepository,
        ICourseRepository courseRepository,
        ICategoryRepository categoryRepository,
        IEnrollmentRepository enrollmentRepository,
        IAdminActionLogRepository logRepository)
    {
        _userRepository = userRepository;
        _courseRepository = courseRepository;
        _categoryRepository = categoryRepository;
        _enrollmentRepository = enrollmentRepository;
        _logRepository = logRepository;
    }

    public async Task<Result<AdminDashboardDto>> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var (totalStudents, activeStudents, blockedStudents) = await _userRepository.GetStudentCountsAsync(cancellationToken);
        var (totalCourses, availableCourses) = await _courseRepository.GetCourseCountsAsync(cancellationToken);
        var totalCategories = await _categoryRepository.GetCountAsync(cancellationToken);
        var totalEnrollments = await _enrollmentRepository.GetTotalCountAsync(cancellationToken);
        var recentEnrollments = await _enrollmentRepository.GetRecentAsync(6, cancellationToken);
        var recentLogs = await _logRepository.GetRecentAsync(8, cancellationToken);

        return Result<AdminDashboardDto>.Success(new AdminDashboardDto
        {
            TotalStudents = totalStudents,
            ActiveStudents = activeStudents,
            BlockedStudents = blockedStudents,
            TotalCourses = totalCourses,
            AvailableCourses = availableCourses,
            TotalCategories = totalCategories,
            TotalEnrollments = totalEnrollments,
            RecentEnrollments = recentEnrollments.Select(EnrollmentService.MapToDto).ToList(),
            RecentLogs = recentLogs.Select(MapLogToDto).ToList()
        });
    }

    public async Task<Result<List<AdminActionLogDto>>> GetActionLogsAsync(int count = 100, CancellationToken cancellationToken = default)
    {
        var logs = await _logRepository.GetRecentAsync(count, cancellationToken);
        return Result<List<AdminActionLogDto>>.Success(logs.Select(MapLogToDto).ToList());
    }

    private static AdminActionLogDto MapLogToDto(Domain.Entities.AdminActionLog log)
    {
        return new AdminActionLogDto
        {
            Id = log.Id,
            AdminId = log.AdminId,
            AdminEmail = log.AdminEmail,
            Action = log.Action,
            EntityName = log.EntityName,
            EntityId = log.EntityId,
            Details = log.Details,
            CreatedAt = log.CreatedAt
        };
    }
}
