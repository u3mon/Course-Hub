using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Extensions;
using Web.Models;

namespace Web.Controllers.Api;

[ApiController]
[Authorize]
[Route("api/me")]
public class MeApiController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public MeApiController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    [HttpGet("courses")]
    public async Task<IActionResult> GetMyCourses(CancellationToken cancellationToken) =>
        (await _enrollmentService.GetStudentCoursesAsync(User.GetUserId(), cancellationToken))
            .ToActionResult();
}
