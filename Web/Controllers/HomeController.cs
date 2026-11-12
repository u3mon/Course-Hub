using System.Diagnostics;
using Application.DTOs.Courses;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Web.Extensions;
using Web.Models;

namespace Web.Controllers;

public class HomeController : Controller
{
    private readonly ICourseService _courseService;
    private readonly ICategoryService _categoryService;

    public HomeController(ICourseService courseService, ICategoryService categoryService)
    {
        _courseService = courseService;
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] CourseFilterDto filter, CancellationToken cancellationToken)
    {
        var coursesResult = await _courseService.GetCatalogAsync(filter, User.GetUserId(), cancellationToken);
        var categoriesResult = await _categoryService.GetAllAsync(cancellationToken);

        return View("~/Views/Courses/Index.cshtml", new CatalogViewModel
        {
            Courses = coursesResult.Value ?? new(),
            Categories = categoriesResult.Value ?? [],
            Filter = filter
        });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
