using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class AdminActionLogRepository : IAdminActionLogRepository
{
    private readonly ApplicationDbContext _context;

    public AdminActionLogRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AdminActionLog log, CancellationToken cancellationToken = default)
    {
        await _context.AdminActionLogs.AddAsync(log, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<AdminActionLog>> GetRecentAsync(int count = 50, CancellationToken cancellationToken = default)
    {
        return await _context.AdminActionLogs
            .AsNoTracking()
            .OrderByDescending(l => l.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }
}
