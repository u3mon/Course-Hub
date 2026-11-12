using Application.Common;
using Application.DTOs.Auth;
using Application.DTOs.Students;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class StudentService : IStudentService
{
    private readonly IUserRepository _userRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IAdminActionLogRepository _logRepository;

    public StudentService(
        IUserRepository userRepository,
        IEnrollmentRepository enrollmentRepository,
        IAdminActionLogRepository logRepository)
    {
        _userRepository = userRepository;
        _enrollmentRepository = enrollmentRepository;
        _logRepository = logRepository;
    }

    public async Task<Result<PagedResult<UserDto>>> GetStudentsPagedAsync(
        StudentFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        if (filter.Page < 1) filter.Page = 1;
        if (filter.PageSize < 1 || filter.PageSize > 50) filter.PageSize = 8;

        var pagedUsers = await _userRepository.GetStudentsPagedAsync(filter, cancellationToken);

        return Result<PagedResult<UserDto>>.Success(new PagedResult<UserDto>
        {
            Items = pagedUsers.Items.Select(MapToUserDto).ToList(),
            TotalCount = pagedUsers.TotalCount,
            PageNumber = pagedUsers.PageNumber,
            PageSize = pagedUsers.PageSize
        });
    }

    public async Task<Result<List<UserDto>>> GetAllActiveStudentsAsync(CancellationToken cancellationToken = default)
    {
        var students = await _userRepository.GetAllActiveStudentsAsync(cancellationToken);
        return Result<List<UserDto>>.Success(students.Select(MapToUserDto).ToList());
    }

    public async Task<Result<StudentDetailDto>> GetStudentByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null || user.Role != UserRole.Student)
        {
            return Result<StudentDetailDto>.Failure("Студент не найден.", ErrorType.NotFound);
        }

        var enrollments = await _enrollmentRepository.GetByUserIdAsync(id, cancellationToken);

        return Result<StudentDetailDto>.Success(new StudentDetailDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            Enrollments = enrollments.Select(EnrollmentService.MapToDto).ToList()
        });
    }

    public async Task<Result<UserDto>> UpdateStudentAsync(
        int id,
        UpdateStudentDto dto,
        int? adminId = null,
        string? adminEmail = null,
        CancellationToken cancellationToken = default)
    {
        var modelCheck = ModelValidator.Validate(dto);
        if (!modelCheck.IsSuccess)
        {
            return Result<UserDto>.Failure(modelCheck.Error!, modelCheck.ErrorType);
        }

        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null || user.Role != UserRole.Student)
        {
            return Result<UserDto>.Failure("Студент не найден.", ErrorType.NotFound);
        }

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        if (await _userRepository.EmailExistsAsync(normalizedEmail, id, cancellationToken))
        {
            return Result<UserDto>.Failure("Пользователь с таким Email уже существует.", ErrorType.Conflict);
        }

        user.FirstName = dto.FirstName.Trim();
        user.LastName = dto.LastName.Trim();
        user.Email = normalizedEmail;
        user.IsActive = dto.IsActive;

        await _userRepository.UpdateAsync(user, cancellationToken);

        if (adminId.HasValue)
        {
            await _logRepository.AddAsync(new AdminActionLog
            {
                AdminId = adminId.Value,
                AdminEmail = adminEmail ?? "admin",
                Action = "Редактирование профиля студента",
                EntityName = "User",
                EntityId = user.Id,
                Details = $"Обновлены данные студента {user.FirstName} {user.LastName} ({user.Email}), статус: {(user.IsActive ? "Активен" : "Заблокирован")}"
            }, cancellationToken);
        }

        return Result<UserDto>.Success(MapToUserDto(user));
    }

    public async Task<Result<UserDto>> ChangeStudentStatusAsync(
        int id,
        bool isActive,
        int? adminId = null,
        string? adminEmail = null,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null || user.Role != UserRole.Student)
        {
            return Result<UserDto>.Failure("Студент не найден.", ErrorType.NotFound);
        }

        user.IsActive = isActive;
        await _userRepository.UpdateAsync(user, cancellationToken);

        if (adminId.HasValue)
        {
            await _logRepository.AddAsync(new AdminActionLog
            {
                AdminId = adminId.Value,
                AdminEmail = adminEmail ?? "admin",
                Action = isActive ? "Разблокировка студента" : "Блокировка студента",
                EntityName = "User",
                EntityId = user.Id,
                Details = $"Студент {user.FirstName} {user.LastName} ({user.Email}) {(isActive ? "восстановлен (активен)" : "заблокирован")}"
            }, cancellationToken);
        }

        return Result<UserDto>.Success(MapToUserDto(user));
    }

    private static UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            EnrolledCoursesCount = user.Enrollments?.Count ?? 0
        };
    }
}
