using Application.Common;
using Application.DTOs.Courses;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CourseRepository : ICourseRepository
{
    private readonly ApplicationDbContext _context;

    public CourseRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<Course>> GetPagedAsync(CourseFilterDto filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Enrollments)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim().ToLower();
            query = query.Where(c =>
                c.Title.ToLower().Contains(term) ||
                c.InstructorName.ToLower().Contains(term) ||
                (c.Description != null && c.Description.ToLower().Contains(term)));
        }

        if (filter.CategoryId.HasValue && filter.CategoryId.Value > 0)
        {
            query = query.Where(c => c.CategoryId == filter.CategoryId.Value);
        }

        if (filter.MinPrice.HasValue)
        {
            query = query.Where(c => c.Price >= filter.MinPrice.Value);
        }

        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(c => c.Price <= filter.MaxPrice.Value);
        }

        if (filter.IsAvailable.HasValue)
        {
            query = query.Where(c => c.IsAvailable == filter.IsAvailable.Value);
        }

        query = (filter.SortBy?.ToLower()) switch
        {
            "title" or "title_asc" => query.OrderBy(c => c.Title),
            "title_desc" => query.OrderByDescending(c => c.Title),
            "price_asc" => query.OrderBy(c => c.Price),
            "price_desc" => query.OrderByDescending(c => c.Price),
            "oldest" => query.OrderBy(c => c.CreatedAt),
            _ => query.OrderByDescending(c => c.CreatedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Course>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<List<Course>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Enrollments)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Course?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<(int Total, int Available)> GetCourseCountsAsync(CancellationToken cancellationToken = default)
    {
        var total = await _context.Courses.AsNoTracking().CountAsync(cancellationToken);
        var available = await _context.Courses.AsNoTracking().CountAsync(c => c.IsAvailable, cancellationToken);
        return (total, available);
    }

    public async Task AddAsync(Course course, CancellationToken cancellationToken = default)
    {
        await _context.Courses.AddAsync(course, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Course course, CancellationToken cancellationToken = default)
    {
        _context.Courses.Update(course);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Course course, CancellationToken cancellationToken = default)
    {
        _context.Courses.Remove(course);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
