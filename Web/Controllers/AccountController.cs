using Application.DTOs.Auth;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Controllers.Api;
using Web.Extensions;
using Web.Models;

namespace Web.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _authService;
    private readonly IEnrollmentService _enrollmentService;

    public AccountController(IAuthService authService, IEnrollmentService enrollmentService)
    {
        _authService = authService;
        _enrollmentService = enrollmentService;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto dto, string? returnUrl = null, CancellationToken cancellationToken = default)
    {
        ViewData["ReturnUrl"] = returnUrl;
        var result = await _authService.LoginAsync(dto, cancellationToken);
        return this.ToAuthFormResult(result, dto, returnUrl);
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterDto());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _authService.StartRegistrationAsync(dto, cancellationToken);
        return this.ToFormResult(
            result,
            dto,
            $"На ваш Email ({dto.Email}) отправлен 6-значный код подтверждения. Введите его для завершения регистрации.",
            nameof(VerifyRegistration),
            routeValues: new { email = dto.Email });
    }

    [HttpGet]
    public IActionResult VerifyRegistration(string? email = null)
    {
        var result = _authService.GetPendingRegistrationState(email);
        return this.ToViewOrRedirect(result, nameof(Register));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyRegistration(VerifyRegistrationDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _authService.VerifyRegistrationCodeAsync(dto, cancellationToken);
        return this.ToAuthFormResult(result, dto, null, "Email успешно подтверждён! Добро пожаловать в CourseHub.");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendRegistrationCode(string email, CancellationToken cancellationToken = default)
    {
        var result = await _authService.ResendRegistrationCodeAsync(email, cancellationToken);
        return this.ToRedirectResult(
            result,
            "Новый 6-значный код отправлен на ваш Email! У вас снова 3 попытки.",
            nameof(VerifyRegistration),
            routeValues: new { email });
    }

    [HttpGet]
    public IActionResult ForgotPassword() => View(new ForgotPasswordDto());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _authService.ForgotPasswordAsync(dto, cancellationToken);
        return this.ToFormResult(
            result,
            dto,
            $"На почту {dto.Email} отправлен код подтверждения. Введите его ниже.",
            nameof(VerifyResetCode),
            routeValues: new { email = dto.Email });
    }

    [HttpGet]
    public IActionResult VerifyResetCode(string? email = null) =>
        View(new VerifyResetCodeDto { Email = email ?? string.Empty });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyResetCode(VerifyResetCodeDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _authService.VerifyResetCodeAsync(dto, cancellationToken);
        return this.ToFormResult(
            result,
            dto,
            "Код успешно подтверждён! Теперь придумайте новый пароль.",
            nameof(ResetPassword),
            routeValues: new { email = dto.Email, code = dto.Code });
    }

    [HttpGet]
    public IActionResult ResetPassword(string? email = null, string? code = null) =>
        View(new ResetPasswordDto { Email = email ?? string.Empty, Code = code ?? string.Empty });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _authService.ResetPasswordAsync(dto, cancellationToken);
        return this.ToFormResult(
            result,
            dto,
            "Ваш пароль успешно изменён! Теперь вы можете войти с новым паролем.",
            nameof(Login));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(AuthApiController.AuthCookieName);
        TempData["Info"] = "Вы вышли из аккаунта.";
        return RedirectToAction("Index", "Courses");
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Profile(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var userResult = await _authService.GetCurrentUserAsync(userId, cancellationToken);
        var enrollmentsResult = await _enrollmentService.GetStudentCoursesAsync(userId, cancellationToken);

        return View(new ProfileViewModel
        {
            User = userResult.Value ?? new(),
            Enrollments = enrollmentsResult.Value ?? []
        });
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View();
    }
}
