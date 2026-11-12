using Application.Common;
using Application.DTOs.Admin;

namespace Application.Interfaces.Services;

public interface IAdminDashboardService
{
    Task<Result<AdminDashboardDto>> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<Result<List<AdminActionLogDto>>> GetActionLogsAsync(int count = 100, CancellationToken cancellationToken = default);
}
