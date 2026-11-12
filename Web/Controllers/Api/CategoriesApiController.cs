using Application.DTOs.Categories;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Extensions;
using Web.Models;

namespace Web.Controllers.Api;

[ApiController]
[Route("api/categories")]
public class CategoriesApiController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesApiController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        (await _categoryService.GetAllAsync(cancellationToken)).ToActionResult();

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken) =>
        (await _categoryService.GetByIdAsync(id, cancellationToken)).ToActionResult();

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto, CancellationToken cancellationToken) =>
        (await _categoryService.CreateAsync(dto, User.GetUserId(), User.GetUserEmail(), cancellationToken))
            .ToActionResult("Категория успешно создана.", StatusCodes.Status201Created);

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto dto, CancellationToken cancellationToken) =>
        (await _categoryService.UpdateAsync(id, dto, User.GetUserId(), User.GetUserEmail(), cancellationToken))
            .ToActionResult("Категория успешно обновлена.");

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken) =>
        (await _categoryService.DeleteAsync(id, User.GetUserId(), User.GetUserEmail(), cancellationToken))
            .ToActionResult("Категория успешно удалена.");
}
