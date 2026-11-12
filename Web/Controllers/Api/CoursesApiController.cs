using Application.DTOs.Courses;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Extensions;
using Web.Models;

namespace Web.Controllers.Api;

[ApiController]
[Route("api/courses")]
public class CoursesApiController : ControllerBase
{
    private readonly ICourseService _courseService;
    private readonly IEnrollmentService _enrollmentService;

    public CoursesApiController(ICourseService courseService, IEnrollmentService enrollmentService)
    {
        _courseService = courseService;
        _enrollmentService = enrollmentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCatalog([FromQuery] CourseFilterDto filter, CancellationToken cancellationToken) =>
        (await _courseService.GetCatalogAsync(filter, User.GetUserId(), cancellationToken))
            .ToActionResult();

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken) =>
        (await _courseService.GetByIdAsync(id, User.GetUserId(), cancellationToken))
            .ToActionResult();

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCourseDto dto, CancellationToken cancellationToken) =>
        (await _courseService.CreateAsync(dto, User.GetUserId(), User.GetUserEmail(), cancellationToken: cancellationToken))
            .ToActionResult("Курс успешно создан.", StatusCodes.Status201Created);

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCourseDto dto, CancellationToken cancellationToken) =>
        (await _courseService.UpdateAsync(id, dto, User.GetUserId(), User.GetUserEmail(), cancellationToken: cancellationToken))
            .ToActionResult("Курс успешно обновлён.");

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken) =>
        (await _courseService.DeleteAsync(id, User.GetUserId(), User.GetUserEmail(), cancellationToken))
            .ToActionResult("Курс успешно удалён.");

    [Authorize(Roles = "Student")]
    [HttpPost("{id:int}/enroll")]
    public async Task<IActionResult> Enroll(int id, CancellationToken cancellationToken) =>
        (await _enrollmentService.EnrollStudentAsync(User.GetUserId(), id, false, null, null, cancellationToken))
            .ToActionResult("Вы успешно записались на курс.", StatusCodes.Status201Created);

    [Authorize(Roles = "Student")]
    [HttpDelete("{id:int}/enroll")]
    public async Task<IActionResult> Unenroll(int id, CancellationToken cancellationToken) =>
        (await _enrollmentService.UnenrollStudentAsync(User.GetUserId(), id, null, null, cancellationToken))
            .ToActionResult("Запись на курс отменена.");
}
