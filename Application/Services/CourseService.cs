using Application.Common;
using Application.DTOs.Courses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;

namespace Application.Services;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IAdminActionLogRepository _logRepository;
    private readonly IFileStorageService _fileStorageService;

    public CourseService(
        ICourseRepository courseRepository,
        ICategoryRepository categoryRepository,
        IEnrollmentRepository enrollmentRepository,
        IAdminActionLogRepository logRepository,
        IFileStorageService fileStorageService)
    {
        _courseRepository = courseRepository;
        _categoryRepository = categoryRepository;
        _enrollmentRepository = enrollmentRepository;
        _logRepository = logRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<PagedResult<CourseDto>>> GetCatalogAsync(
        CourseFilterDto filter,
        int? currentUserId = null,
        CancellationToken cancellationToken = default)
    {
        if (filter.Page < 1) filter.Page = 1;
        if (filter.PageSize < 1 || filter.PageSize > 50) filter.PageSize = 6;

        var pagedCourses = await _courseRepository.GetPagedAsync(filter, cancellationToken);

        HashSet<int> enrolledCourseIds = [];
        if (currentUserId.HasValue)
        {
            var userEnrollments = await _enrollmentRepository.GetByUserIdAsync(currentUserId.Value, cancellationToken);
            enrolledCourseIds = userEnrollments.Select(e => e.CourseId).ToHashSet();
        }

        var dtoItems = pagedCourses.Items
            .Select(c => MapToDto(c, enrolledCourseIds.Contains(c.Id)))
            .ToList();

        return Result<PagedResult<CourseDto>>.Success(new PagedResult<CourseDto>
        {
            Items = dtoItems,
            TotalCount = pagedCourses.TotalCount,
            PageNumber = pagedCourses.PageNumber,
            PageSize = pagedCourses.PageSize
        });
    }

    public async Task<Result<List<CourseDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var courses = await _courseRepository.GetAllAsync(cancellationToken);
        return Result<List<CourseDto>>.Success(courses.Select(c => MapToDto(c, false)).ToList());
    }

    public async Task<Result<CourseDto>> GetByIdAsync(int id, int? currentUserId = null, CancellationToken cancellationToken = default)
    {
        var course = await _courseRepository.GetByIdWithDetailsAsync(id, cancellationToken);
        if (course is null)
        {
            return Result<CourseDto>.Failure("Курс не найден.", ErrorType.NotFound);
        }

        var isEnrolled = false;
        if (currentUserId.HasValue)
        {
            isEnrolled = await _enrollmentRepository.IsEnrolledAsync(currentUserId.Value, id, cancellationToken);
        }

        return Result<CourseDto>.Success(MapToDto(course, isEnrolled));
    }

    public async Task<Result<CourseDto>> CreateAsync(
        CreateCourseDto dto,
        int? adminId = null,
        string? adminEmail = null,
        Stream? coverStream = null,
        string? coverFileName = null,
        long coverLength = 0,
        CancellationToken cancellationToken = default)
    {
        var modelCheck = ModelValidator.Validate(dto);
        if (!modelCheck.IsSuccess)
        {
            return Result<CourseDto>.Failure(modelCheck.Error!, modelCheck.ErrorType);
        }

        var coverResult = await _fileStorageService.SaveCourseCoverAsync(coverStream, coverFileName, coverLength, cancellationToken);
        if (!coverResult.IsSuccess)
        {
            return Result<CourseDto>.Failure(coverResult.Error!, coverResult.ErrorType);
        }

        if (!string.IsNullOrWhiteSpace(coverResult.Value))
        {
            dto.ImageUrl = coverResult.Value;
        }

        var validationError = ValidateCourseInput(dto.Title, dto.Description, dto.InstructorName, dto.Duration, dto.Price, dto.MaxStudents);
        if (validationError is not null)
        {
            return Result<CourseDto>.Failure(validationError, ErrorType.Validation);
        }

        var category = await _categoryRepository.GetByIdAsync(dto.CategoryId, cancellationToken);
        if (category is null)
        {
            return Result<CourseDto>.Failure("Выбранная категория не существует.", ErrorType.Validation);
        }

        var course = new Course
        {
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            CategoryId = dto.CategoryId,
            InstructorName = dto.InstructorName.Trim(),
            Duration = dto.Duration.Trim(),
            Price = dto.Price,
            IsAvailable = dto.IsAvailable,
            ImageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl) ? null : dto.ImageUrl.Trim(),
            MaxStudents = dto.MaxStudents,
            CreatedAt = DateTime.UtcNow
        };

        await _courseRepository.AddAsync(course, cancellationToken);
        course.Category = category;

        if (adminId.HasValue)
        {
            await _logRepository.AddAsync(new AdminActionLog
            {
                AdminId = adminId.Value,
                AdminEmail = adminEmail ?? "admin",
                Action = "Создание курса",
                EntityName = "Course",
                EntityId = course.Id,
                Details = $"Создан курс «{course.Title}» (категория: {category.Name}, цена: {course.Price:N0} ₽)"
            }, cancellationToken);
        }

        return Result<CourseDto>.Success(MapToDto(course, false));
    }

    public async Task<Result<CourseDto>> UpdateAsync(
        int id,
        UpdateCourseDto dto,
        int? adminId = null,
        string? adminEmail = null,
        Stream? coverStream = null,
        string? coverFileName = null,
        long coverLength = 0,
        CancellationToken cancellationToken = default)
    {
        var modelCheck = ModelValidator.Validate(dto);
        if (!modelCheck.IsSuccess)
        {
            return Result<CourseDto>.Failure(modelCheck.Error!, modelCheck.ErrorType);
        }

        var coverResult = await _fileStorageService.SaveCourseCoverAsync(coverStream, coverFileName, coverLength, cancellationToken);
        if (!coverResult.IsSuccess)
        {
            return Result<CourseDto>.Failure(coverResult.Error!, coverResult.ErrorType);
        }

        if (!string.IsNullOrWhiteSpace(coverResult.Value))
        {
            dto.ImageUrl = coverResult.Value;
        }

        var validationError = ValidateCourseInput(dto.Title, dto.Description, dto.InstructorName, dto.Duration, dto.Price, dto.MaxStudents);
        if (validationError is not null)
        {
            return Result<CourseDto>.Failure(validationError, ErrorType.Validation);
        }

        var course = await _courseRepository.GetByIdWithDetailsAsync(id, cancellationToken);
        if (course is null)
        {
            return Result<CourseDto>.Failure("Курс не найден.", ErrorType.NotFound);
        }

        var category = await _categoryRepository.GetByIdAsync(dto.CategoryId, cancellationToken);
        if (category is null)
        {
            return Result<CourseDto>.Failure("Выбранная категория не существует.", ErrorType.Validation);
        }

        course.Title = dto.Title.Trim();
        course.Description = dto.Description.Trim();
        course.CategoryId = dto.CategoryId;
        course.Category = category;
        course.InstructorName = dto.InstructorName.Trim();
        course.Duration = dto.Duration.Trim();
        course.Price = dto.Price;
        course.IsAvailable = dto.IsAvailable;
        course.ImageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl) ? null : dto.ImageUrl.Trim();
        course.MaxStudents = dto.MaxStudents;

        await _courseRepository.UpdateAsync(course, cancellationToken);

        if (adminId.HasValue)
        {
            await _logRepository.AddAsync(new AdminActionLog
            {
                AdminId = adminId.Value,
                AdminEmail = adminEmail ?? "admin",
                Action = "Редактирование курса",
                EntityName = "Course",
                EntityId = course.Id,
                Details = $"Обновлён курс «{course.Title}»"
            }, cancellationToken);
        }

        return Result<CourseDto>.Success(MapToDto(course, false));
    }

    public async Task<Result> DeleteAsync(
        int id,
        int? adminId = null,
        string? adminEmail = null,
        CancellationToken cancellationToken = default)
    {
        var course = await _courseRepository.GetByIdWithDetailsAsync(id, cancellationToken);
        if (course is null)
        {
            return Result.Failure("Курс не найден.", ErrorType.NotFound);
        }

        var courseTitle = course.Title;
        await _courseRepository.DeleteAsync(course, cancellationToken);

        if (adminId.HasValue)
        {
            await _logRepository.AddAsync(new AdminActionLog
            {
                AdminId = adminId.Value,
                AdminEmail = adminEmail ?? "admin",
                Action = "Удаление курса",
                EntityName = "Course",
                EntityId = id,
                Details = $"Удалён курс «{courseTitle}»"
            }, cancellationToken);
        }

        return Result.Success();
    }

    public async Task<Result<CourseDto>> ToggleAvailabilityAsync(
        int id,
        int? adminId = null,
        string? adminEmail = null,
        CancellationToken cancellationToken = default)
    {
        var course = await _courseRepository.GetByIdWithDetailsAsync(id, cancellationToken);
        if (course is null)
        {
            return Result<CourseDto>.Failure("Курс не найден.", ErrorType.NotFound);
        }

        course.IsAvailable = !course.IsAvailable;
        await _courseRepository.UpdateAsync(course, cancellationToken);

        if (adminId.HasValue)
        {
            await _logRepository.AddAsync(new AdminActionLog
            {
                AdminId = adminId.Value,
                AdminEmail = adminEmail ?? "admin",
                Action = "Изменение доступности курса",
                EntityName = "Course",
                EntityId = course.Id,
                Details = $"Курс «{course.Title}» переведён в статус: {(course.IsAvailable ? "Доступен" : "Скрыт/Недоступен")}"
            }, cancellationToken);
        }

        return Result<CourseDto>.Success(MapToDto(course, false));
    }

    private static string? ValidateCourseInput(
        string? title,
        string? description,
        string? instructorName,
        string? duration,
        decimal price,
        int? maxStudents)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length < 3)
            return "Название курса должно содержать не менее 3 символов.";
        if (string.IsNullOrWhiteSpace(description) || description.Trim().Length < 10)
            return "Описание курса обязательно и должно содержать не менее 10 символов.";
        if (string.IsNullOrWhiteSpace(instructorName) || instructorName.Trim().Length < 2)
            return "Имя преподавателя обязательно для заполнения.";
        if (string.IsNullOrWhiteSpace(duration) || duration.Trim().Length < 2)
            return "Продолжительность курса обязательна для заполнения.";
        if (price < 0)
            return "Стоимость курса не может быть отрицательной.";
        if (maxStudents.HasValue && maxStudents.Value < 1)
            return "Максимальное количество студентов должно быть больше 0.";

        return null;
    }

    private static CourseDto MapToDto(Course course, bool isEnrolled)
    {
        return new CourseDto
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            CategoryId = course.CategoryId,
            CategoryName = course.Category?.Name ?? string.Empty,
            InstructorName = course.InstructorName,
            Duration = course.Duration,
            Price = course.Price,
            IsAvailable = course.IsAvailable,
            ImageUrl = course.ImageUrl,
            MaxStudents = course.MaxStudents,
            EnrolledCount = course.Enrollments?.Count ?? 0,
            IsEnrolled = isEnrolled,
            CreatedAt = course.CreatedAt
        };
    }
}
