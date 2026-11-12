using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Extensions;

namespace Web.Controllers;

[Authorize]
public class EnrollmentsController : Controller
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentsController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    [HttpGet]
    public async Task<IActionResult> MyCourses(CancellationToken cancellationToken)
    {
        var result = await _enrollmentService.GetStudentCoursesAsync(User.GetUserId(), cancellationToken);
        return this.ToViewOrRedirect(result, "Login", "Account");
    }

    [Authorize(Roles = "Student")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(int courseId, string? returnUrl = null, CancellationToken cancellationToken = default)
    {
        var result = await _enrollmentService.EnrollStudentAsync(User.GetUserId(), courseId, false, null, null, cancellationToken);
        return this.ToRedirectResult(result, "Вы успешно записались на курс!", "Details", "Courses", new { id = courseId }, returnUrl);
    }

    [Authorize(Roles = "Student")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unenroll(int courseId, string? returnUrl = null, CancellationToken cancellationToken = default)
    {
        var result = await _enrollmentService.UnenrollStudentAsync(User.GetUserId(), courseId, null, null, cancellationToken);
        return this.ToRedirectResult(result, "Ваша запись на курс отменена.", nameof(MyCourses), null, null, returnUrl, "Info");
    }
}
