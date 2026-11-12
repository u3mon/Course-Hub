using Application.DTOs.Courses;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Web.Extensions;
using Web.Models;

namespace Web.Controllers;

public class CoursesController : Controller
{
    private readonly ICourseService _courseService;
    private readonly ICategoryService _categoryService;

    public CoursesController(ICourseService courseService, ICategoryService categoryService)
    {
        _courseService = courseService;
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] CourseFilterDto filter, CancellationToken cancellationToken)
    {
        var coursesResult = await _courseService.GetCatalogAsync(filter, User.GetUserId(), cancellationToken);
        var categoriesResult = await _categoryService.GetAllAsync(cancellationToken);

        return View(new CatalogViewModel
        {
            Courses = coursesResult.Value ?? new(),
            Categories = categoriesResult.Value ?? [],
            Filter = filter
        });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var result = await _courseService.GetByIdAsync(id, User.GetUserId(), cancellationToken);
        return this.ToViewOrRedirect(result, nameof(Index));
    }
}
