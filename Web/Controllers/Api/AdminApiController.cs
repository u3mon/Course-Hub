using Application.DTOs.Students;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Extensions;
using Web.Models;

namespace Web.Controllers.Api;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin")]
public class AdminApiController : ControllerBase
{
    private readonly IStudentService _studentService;
    private readonly IEnrollmentService _enrollmentService;
    private readonly IAdminDashboardService _dashboardService;

    public AdminApiController(
        IStudentService studentService,
        IEnrollmentService enrollmentService,
        IAdminDashboardService dashboardService)
    {
        _studentService = studentService;
        _enrollmentService = enrollmentService;
        _dashboardService = dashboardService;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken) =>
        (await _dashboardService.GetDashboardAsync(cancellationToken)).ToActionResult();

    [HttpGet("students")]
    public async Task<IActionResult> GetStudents([FromQuery] StudentFilterDto filter, CancellationToken cancellationToken) =>
        (await _studentService.GetStudentsPagedAsync(filter, cancellationToken)).ToActionResult();

    [HttpGet("students/{id:int}")]
    public async Task<IActionResult> GetStudentById(int id, CancellationToken cancellationToken) =>
        (await _studentService.GetStudentByIdAsync(id, cancellationToken)).ToActionResult();

    [HttpPut("students/{id:int}")]
    public async Task<IActionResult> UpdateStudent(int id, [FromBody] UpdateStudentDto dto, CancellationToken cancellationToken) =>
        (await _studentService.UpdateStudentAsync(id, dto, User.GetUserId(), User.GetUserEmail(), cancellationToken))
            .ToActionResult("Данные студента обновлены.");

    [HttpPatch("students/{id:int}/status")]
    public async Task<IActionResult> ChangeStudentStatus(int id, [FromBody] UpdateStudentStatusDto dto, CancellationToken cancellationToken) =>
        (await _studentService.ChangeStudentStatusAsync(id, dto.IsActive, User.GetUserId(), User.GetUserEmail(), cancellationToken))
            .ToActionResult("Статус аккаунта студента обновлён.");

    [HttpGet("courses/{id:int}/students")]
    public async Task<IActionResult> GetCourseStudents(int id, CancellationToken cancellationToken) =>
        (await _enrollmentService.GetCourseStudentsAsync(id, cancellationToken)).ToActionResult();

    [HttpPost("courses/{id:int}/students/{studentId:int}")]
    public async Task<IActionResult> EnrollStudentInCourse(int id, int studentId, CancellationToken cancellationToken) =>
        (await _enrollmentService.EnrollStudentAsync(
            studentId,
            id,
            isAdminOverride: true,
            adminId: User.GetUserId(),
            adminEmail: User.GetUserEmail(),
            cancellationToken: cancellationToken))
            .ToActionResult("Студент записан на курс.", StatusCodes.Status201Created);

    [HttpDelete("courses/{id:int}/students/{studentId:int}")]
    public async Task<IActionResult> RemoveStudentFromCourse(int id, int studentId, CancellationToken cancellationToken) =>
        (await _enrollmentService.UnenrollStudentAsync(
            studentId,
            id,
            adminId: User.GetUserId(),
            adminEmail: User.GetUserEmail(),
            cancellationToken: cancellationToken))
            .ToActionResult("Запись студента на курс удалена.");
}
