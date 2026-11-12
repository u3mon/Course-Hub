using Application.Common;
using Application.DTOs.Categories;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;

namespace Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IAdminActionLogRepository _logRepository;

    public CategoryService(
        ICategoryRepository categoryRepository,
        IAdminActionLogRepository logRepository)
    {
        _categoryRepository = categoryRepository;
        _logRepository = logRepository;
    }

    public async Task<Result<List<CategoryDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _categoryRepository.GetAllWithCoursesCountAsync(cancellationToken);
        var dtos = categories.Select(MapToDto).ToList();
        return Result<List<CategoryDto>>.Success(dtos);
    }

    public async Task<Result<CategoryDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
        if (category is null)
        {
            return Result<CategoryDto>.Failure("Категория не найдена.", ErrorType.NotFound);
        }

        return Result<CategoryDto>.Success(MapToDto(category));
    }

    public async Task<Result<CategoryDto>> CreateAsync(CreateCategoryDto dto, int? adminId = null, string? adminEmail = null, CancellationToken cancellationToken = default)
    {
        var modelCheck = ModelValidator.Validate(dto);
        if (!modelCheck.IsSuccess)
        {
            return Result<CategoryDto>.Failure(modelCheck.Error!, modelCheck.ErrorType);
        }

        var trimmedName = dto.Name.Trim();
        if (await _categoryRepository.ExistsByNameAsync(trimmedName, null, cancellationToken))
        {
            return Result<CategoryDto>.Failure("Категория с таким названием уже существует.", ErrorType.Conflict);
        }

        var category = new Category
        {
            Name = trimmedName,
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim()
        };

        await _categoryRepository.AddAsync(category, cancellationToken);

        if (adminId.HasValue)
        {
            await _logRepository.AddAsync(new AdminActionLog
            {
                AdminId = adminId.Value,
                AdminEmail = adminEmail ?? "admin",
                Action = "Создание категории",
                EntityName = "Category",
                EntityId = category.Id,
                Details = $"Создана категория «{category.Name}»"
            }, cancellationToken);
        }

        return Result<CategoryDto>.Success(MapToDto(category));
    }

    public async Task<Result<CategoryDto>> UpdateAsync(int id, UpdateCategoryDto dto, int? adminId = null, string? adminEmail = null, CancellationToken cancellationToken = default)
    {
        var modelCheck = ModelValidator.Validate(dto);
        if (!modelCheck.IsSuccess)
        {
            return Result<CategoryDto>.Failure(modelCheck.Error!, modelCheck.ErrorType);
        }

        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
        if (category is null)
        {
            return Result<CategoryDto>.Failure("Категория не найдена.", ErrorType.NotFound);
        }

        var trimmedName = dto.Name.Trim();
        if (await _categoryRepository.ExistsByNameAsync(trimmedName, id, cancellationToken))
        {
            return Result<CategoryDto>.Failure("Категория с таким названием уже существует.", ErrorType.Conflict);
        }

        category.Name = trimmedName;
        category.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();

        await _categoryRepository.UpdateAsync(category, cancellationToken);

        if (adminId.HasValue)
        {
            await _logRepository.AddAsync(new AdminActionLog
            {
                AdminId = adminId.Value,
                AdminEmail = adminEmail ?? "admin",
                Action = "Редактирование категории",
                EntityName = "Category",
                EntityId = category.Id,
                Details = $"Обновлена категория «{category.Name}»"
            }, cancellationToken);
        }

        return Result<CategoryDto>.Success(MapToDto(category));
    }

    public async Task<Result> DeleteAsync(int id, int? adminId = null, string? adminEmail = null, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
        if (category is null)
        {
            return Result.Failure("Категория не найдена.", ErrorType.NotFound);
        }

        if (await _categoryRepository.HasCoursesAsync(id, cancellationToken))
        {
            return Result.Failure(
                $"Невозможно удалить категорию «{category.Name}», так как в ней содержатся курсы. Сначала переместите или удалите связанные курсы.",
                ErrorType.Conflict);
        }

        var categoryName = category.Name;
        await _categoryRepository.DeleteAsync(category, cancellationToken);

        if (adminId.HasValue)
        {
            await _logRepository.AddAsync(new AdminActionLog
            {
                AdminId = adminId.Value,
                AdminEmail = adminEmail ?? "admin",
                Action = "Удаление категории",
                EntityName = "Category",
                EntityId = id,
                Details = $"Удалена категория «{categoryName}»"
            }, cancellationToken);
        }

        return Result.Success();
    }

    private static CategoryDto MapToDto(Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            CoursesCount = category.Courses?.Count ?? 0
        };
    }
}
