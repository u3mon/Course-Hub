using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IAdminActionLogRepository
{
    Task AddAsync(AdminActionLog log, CancellationToken cancellationToken = default);
    Task<List<AdminActionLog>> GetRecentAsync(int count = 50, CancellationToken cancellationToken = default);
}
