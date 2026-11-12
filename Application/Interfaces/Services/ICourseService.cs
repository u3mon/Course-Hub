using Application.Common;
using Application.DTOs.Courses;

namespace Application.Interfaces.Services;

public interface ICourseService
{
    Task<Result<PagedResult<CourseDto>>> GetCatalogAsync(CourseFilterDto filter, int? currentUserId = null, CancellationToken cancellationToken = default);
    Task<Result<List<CourseDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<CourseDto>> GetByIdAsync(int id, int? currentUserId = null, CancellationToken cancellationToken = default);
    Task<Result<CourseDto>> CreateAsync(
        CreateCourseDto dto,
        int? adminId = null,
        string? adminEmail = null,
        Stream? coverStream = null,
        string? coverFileName = null,
        long coverLength = 0,
        CancellationToken cancellationToken = default);
    Task<Result<CourseDto>> UpdateAsync(
        int id,
        UpdateCourseDto dto,
        int? adminId = null,
        string? adminEmail = null,
        Stream? coverStream = null,
        string? coverFileName = null,
        long coverLength = 0,
        CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int id, int? adminId = null, string? adminEmail = null, CancellationToken cancellationToken = default);
    Task<Result<CourseDto>> ToggleAvailabilityAsync(int id, int? adminId = null, string? adminEmail = null, CancellationToken cancellationToken = default);
}
