using Application.Common;
using Application.DTOs.Courses;
using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface ICourseRepository
{
    Task<PagedResult<Course>> GetPagedAsync(CourseFilterDto filter, CancellationToken cancellationToken = default);
    Task<List<Course>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Course?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<(int Total, int Available)> GetCourseCountsAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Course course, CancellationToken cancellationToken = default);
    Task UpdateAsync(Course course, CancellationToken cancellationToken = default);
    Task DeleteAsync(Course course, CancellationToken cancellationToken = default);
}
