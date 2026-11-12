using Application.Common;
using Application.DTOs.Auth;
using Application.DTOs.Students;

namespace Application.Interfaces.Services;

public interface IStudentService
{
    Task<Result<PagedResult<UserDto>>> GetStudentsPagedAsync(StudentFilterDto filter, CancellationToken cancellationToken = default);
    Task<Result<List<UserDto>>> GetAllActiveStudentsAsync(CancellationToken cancellationToken = default);
    Task<Result<StudentDetailDto>> GetStudentByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> UpdateStudentAsync(int id, UpdateStudentDto dto, int? adminId = null, string? adminEmail = null, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> ChangeStudentStatusAsync(int id, bool isActive, int? adminId = null, string? adminEmail = null, CancellationToken cancellationToken = default);
}
