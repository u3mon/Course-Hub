using Application.Common;
using Application.DTOs.Enrollments;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAdminActionLogRepository _logRepository;

    public EnrollmentService(
        IEnrollmentRepository enrollmentRepository,
        ICourseRepository courseRepository,
        IUserRepository userRepository,
        IAdminActionLogRepository logRepository)
    {
        _enrollmentRepository = enrollmentRepository;
        _courseRepository = courseRepository;
        _userRepository = userRepository;
        _logRepository = logRepository;
    }

    public async Task<Result<EnrollmentDto>> EnrollStudentAsync(
        int? userId,
        int courseId,
        bool isAdminOverride = false,
        int? adminId = null,
        string? adminEmail = null,
        CancellationToken cancellationToken = default)
    {
        if (!userId.HasValue || userId.Value <= 0)
        {
            return Result<EnrollmentDto>.Failure("Необходимо войти в систему.", ErrorType.Unauthorized);
        }

        var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
        if (user is null)
        {
            return Result<EnrollmentDto>.Failure("Пользователь не найден.", ErrorType.NotFound);
        }

        if (!user.IsActive)
        {
            return Result<EnrollmentDto>.Failure("Заблокированный пользователь не может быть записан на курс.", ErrorType.Forbidden);
        }

        if (!isAdminOverride && user.Role != UserRole.Student)
        {
            return Result<EnrollmentDto>.Failure("Только пользователи с ролью Student могут записываться на курсы.", ErrorType.Forbidden);
        }

        var course = await _courseRepository.GetByIdWithDetailsAsync(courseId, cancellationToken);
        if (course is null)
        {
            return Result<EnrollmentDto>.Failure("Курс не найден.", ErrorType.NotFound);
        }

        if (!course.IsAvailable && !isAdminOverride)
        {
            return Result<EnrollmentDto>.Failure("Данный курс сейчас недоступен для записи.", ErrorType.BadRequest);
        }

        if (await _enrollmentRepository.IsEnrolledAsync(userId.Value, courseId, cancellationToken))
        {
            return Result<EnrollmentDto>.Failure("Студент уже записан на этот курс.", ErrorType.Conflict);
        }

        var currentCount = course.Enrollments?.Count ?? await _enrollmentRepository.GetCourseEnrollmentCountAsync(courseId, cancellationToken);
        if (course.MaxStudents.HasValue && currentCount >= course.MaxStudents.Value)
        {
            return Result<EnrollmentDto>.Failure($"На курсе «{course.Title}» достигнут лимит мест ({course.MaxStudents.Value}).", ErrorType.Conflict);
        }

        var enrollment = new Enrollment
        {
            UserId = userId.Value,
            CourseId = courseId,
            EnrollDate = DateTime.UtcNow
        };

        await _enrollmentRepository.AddAsync(enrollment, cancellationToken);
        enrollment.User = user;
        enrollment.Course = course;

        if (adminId.HasValue)
        {
            await _logRepository.AddAsync(new AdminActionLog
            {
                AdminId = adminId.Value,
                AdminEmail = adminEmail ?? "admin",
                Action = "Запись студента на курс",
                EntityName = "Enrollment",
                EntityId = enrollment.Id,
                Details = $"Студент {user.FirstName} {user.LastName} ({user.Email}) записан на курс «{course.Title}»"
            }, cancellationToken);
        }

        return Result<EnrollmentDto>.Success(MapToDto(enrollment));
    }

    public async Task<Result> UnenrollStudentAsync(
        int? userId,
        int courseId,
        int? adminId = null,
        string? adminEmail = null,
        CancellationToken cancellationToken = default)
    {
        if (!userId.HasValue || userId.Value <= 0)
        {
            return Result.Failure("Необходимо войти в систему.", ErrorType.Unauthorized);
        }

        var enrollment = await _enrollmentRepository.GetByUserAndCourseAsync(userId.Value, courseId, cancellationToken);
        if (enrollment is null)
        {
            return Result.Failure("Запись на курс не найдена.", ErrorType.NotFound);
        }

        var studentInfo = enrollment.User is not null
            ? $"{enrollment.User.FirstName} {enrollment.User.LastName} ({enrollment.User.Email})"
            : $"ID={userId.Value}";
        var courseTitle = enrollment.Course?.Title ?? $"ID={courseId}";

        await _enrollmentRepository.DeleteAsync(enrollment, cancellationToken);

        if (adminId.HasValue)
        {
            await _logRepository.AddAsync(new AdminActionLog
            {
                AdminId = adminId.Value,
                AdminEmail = adminEmail ?? "admin",
                Action = "Отмена записи на курс",
                EntityName = "Enrollment",
                EntityId = courseId,
                Details = $"Удалена запись студента {studentInfo} с курса «{courseTitle}»"
            }, cancellationToken);
        }

        return Result.Success();
    }

    public async Task<Result<List<EnrollmentDto>>> GetStudentCoursesAsync(int? userId, CancellationToken cancellationToken = default)
    {
        if (!userId.HasValue || userId.Value <= 0)
        {
            return Result<List<EnrollmentDto>>.Failure("Необходимо войти в систему.", ErrorType.Unauthorized);
        }

        var enrollments = await _enrollmentRepository.GetByUserIdAsync(userId.Value, cancellationToken);
        return Result<List<EnrollmentDto>>.Success(enrollments.Select(MapToDto).ToList());
    }

    public async Task<Result<List<EnrollmentDto>>> GetCourseStudentsAsync(int courseId, CancellationToken cancellationToken = default)
    {
        var course = await _courseRepository.GetByIdWithDetailsAsync(courseId, cancellationToken);
        if (course is null)
        {
            return Result<List<EnrollmentDto>>.Failure("Курс не найден.", ErrorType.NotFound);
        }

        var enrollments = await _enrollmentRepository.GetByCourseIdAsync(courseId, cancellationToken);
        return Result<List<EnrollmentDto>>.Success(enrollments.Select(MapToDto).ToList());
    }

    public static EnrollmentDto MapToDto(Enrollment enrollment)
    {
        return new EnrollmentDto
        {
            Id = enrollment.Id,
            CourseId = enrollment.CourseId,
            CourseTitle = enrollment.Course?.Title ?? string.Empty,
            CategoryName = enrollment.Course?.Category?.Name ?? string.Empty,
            InstructorName = enrollment.Course?.InstructorName ?? string.Empty,
            Duration = enrollment.Course?.Duration ?? string.Empty,
            Price = enrollment.Course?.Price ?? 0,
            ImageUrl = enrollment.Course?.ImageUrl,
            IsCourseAvailable = enrollment.Course?.IsAvailable ?? true,
            UserId = enrollment.UserId,
            StudentFirstName = enrollment.User?.FirstName ?? string.Empty,
            StudentLastName = enrollment.User?.LastName ?? string.Empty,
            StudentEmail = enrollment.User?.Email ?? string.Empty,
            StudentIsActive = enrollment.User?.IsActive ?? true,
            EnrollDate = enrollment.EnrollDate
        };
    }
}
