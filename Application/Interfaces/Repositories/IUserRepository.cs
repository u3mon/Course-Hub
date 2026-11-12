using Application.Common;
using Application.DTOs.Students;
using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, int? excludeUserId = null, CancellationToken cancellationToken = default);
    Task<PagedResult<User>> GetStudentsPagedAsync(StudentFilterDto filter, CancellationToken cancellationToken = default);
    Task<List<User>> GetAllActiveStudentsAsync(CancellationToken cancellationToken = default);
    Task<(int Total, int Active, int Blocked)> GetStudentCountsAsync(CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
}
