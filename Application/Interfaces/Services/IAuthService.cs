using Application.Common;
using Application.DTOs.Auth;

namespace Application.Interfaces.Services;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default);
    Task<Result> StartRegistrationAsync(RegisterDto dto, CancellationToken cancellationToken = default);
    Result<VerifyRegistrationDto> GetPendingRegistrationState(string? email);
    Task<Result<AuthResponseDto>> VerifyRegistrationCodeAsync(VerifyRegistrationDto dto, CancellationToken cancellationToken = default);
    Task<Result> ResendRegistrationCodeAsync(string? email, CancellationToken cancellationToken = default);
    Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> GetCurrentUserAsync(int? userId, CancellationToken cancellationToken = default);
    Task<Result> ForgotPasswordAsync(ForgotPasswordDto dto, CancellationToken cancellationToken = default);
    Task<Result> VerifyResetCodeAsync(VerifyResetCodeDto dto, CancellationToken cancellationToken = default);
    Task<Result> ResetPasswordAsync(ResetPasswordDto dto, CancellationToken cancellationToken = default);
}
