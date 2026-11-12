using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly ApplicationDbContext _context;

    public EnrollmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Enrollment?> GetByUserAndCourseAsync(int userId, int courseId, CancellationToken cancellationToken = default)
    {
        return await _context.Enrollments
            .Include(e => e.User)
            .Include(e => e.Course)
                .ThenInclude(c => c.Category)
            .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId, cancellationToken);
    }

    public async Task<bool> IsEnrolledAsync(int userId, int courseId, CancellationToken cancellationToken = default)
    {
        return await _context.Enrollments
            .AsNoTracking()
            .AnyAsync(e => e.UserId == userId && e.CourseId == courseId, cancellationToken);
    }

    public async Task<int> GetCourseEnrollmentCountAsync(int courseId, CancellationToken cancellationToken = default)
    {
        return await _context.Enrollments
            .AsNoTracking()
            .CountAsync(e => e.CourseId == courseId, cancellationToken);
    }

    public async Task<List<Enrollment>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.Enrollments
            .Include(e => e.User)
            .Include(e => e.Course)
                .ThenInclude(c => c.Category)
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.EnrollDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Enrollment>> GetByCourseIdAsync(int courseId, CancellationToken cancellationToken = default)
    {
        return await _context.Enrollments
            .Include(e => e.User)
            .Include(e => e.Course)
                .ThenInclude(c => c.Category)
            .Where(e => e.CourseId == courseId)
            .OrderByDescending(e => e.EnrollDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Enrollment>> GetRecentAsync(int count, CancellationToken cancellationToken = default)
    {
        return await _context.Enrollments
            .Include(e => e.User)
            .Include(e => e.Course)
                .ThenInclude(c => c.Category)
            .OrderByDescending(e => e.EnrollDate)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Enrollments.AsNoTracking().CountAsync(cancellationToken);
    }

    public async Task AddAsync(Enrollment enrollment, CancellationToken cancellationToken = default)
    {
        await _context.Enrollments.AddAsync(enrollment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Enrollment enrollment, CancellationToken cancellationToken = default)
    {
        _context.Enrollments.Remove(enrollment);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
