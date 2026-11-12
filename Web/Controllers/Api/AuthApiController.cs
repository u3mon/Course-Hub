using Application.DTOs.Auth;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Extensions;
using Web.Models;

namespace Web.Controllers.Api;

[ApiController]
[Route("api/auth")]
public class AuthApiController : ControllerBase
{
    public const string AuthCookieName = "CourseHub.AuthToken";
    private readonly IAuthService _authService;

    public AuthApiController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken cancellationToken) =>
        (await _authService.StartRegistrationAsync(dto, cancellationToken))
            .ToActionResult("Код подтверждения регистрации отправлен на ваш Email.");

    [HttpPost("register/verify")]
    public async Task<IActionResult> VerifyRegistration([FromBody] VerifyRegistrationDto dto, CancellationToken cancellationToken) =>
        (await _authService.VerifyRegistrationCodeAsync(dto, cancellationToken))
            .WithAuthCookie(Response, Request.IsHttps)
            .ToActionResult("Регистрация успешно завершена.", StatusCodes.Status201Created);

    [HttpPost("register/resend-code")]
    public async Task<IActionResult> ResendRegistrationCode([FromBody] ForgotPasswordDto dto, CancellationToken cancellationToken) =>
        (await _authService.ResendRegistrationCodeAsync(dto.Email, cancellationToken))
            .ToActionResult("Новый код подтверждения отправлен на ваш Email.");

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken cancellationToken) =>
        (await _authService.LoginAsync(dto, cancellationToken))
            .WithAuthCookie(Response, Request.IsHttps)
            .ToActionResult("Вход выполнен успешно.");

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto, CancellationToken cancellationToken) =>
        (await _authService.ForgotPasswordAsync(dto, cancellationToken))
            .ToActionResult("Код для сброса пароля отправлен на указанный Email.");

    [HttpPost("verify-reset-code")]
    public async Task<IActionResult> VerifyResetCode([FromBody] VerifyResetCodeDto dto, CancellationToken cancellationToken) =>
        (await _authService.VerifyResetCodeAsync(dto, cancellationToken))
            .ToActionResult("Код подтверждён. Теперь вы можете задать новый пароль.");

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto, CancellationToken cancellationToken) =>
        (await _authService.ResetPasswordAsync(dto, cancellationToken))
            .ToActionResult("Пароль успешно изменён. Теперь вы можете войти с новым паролем.");

    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(AuthCookieName);
        return Ok(ApiResponse.Ok("Вы вышли из аккаунта."));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken) =>
        (await _authService.GetCurrentUserAsync(User.GetUserId(), cancellationToken))
            .ToActionResult();
}
