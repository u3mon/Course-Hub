using Application.Common;
using Application.DTOs.Categories;

namespace Application.Interfaces.Services;

public interface ICategoryService
{
    Task<Result<List<CategoryDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<CategoryDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<CategoryDto>> CreateAsync(CreateCategoryDto dto, int? adminId = null, string? adminEmail = null, CancellationToken cancellationToken = default);
    Task<Result<CategoryDto>> UpdateAsync(int id, UpdateCategoryDto dto, int? adminId = null, string? adminEmail = null, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int id, int? adminId = null, string? adminEmail = null, CancellationToken cancellationToken = default);
}
