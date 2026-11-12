using Application.Common;
using Application.DTOs.Auth;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;

namespace Application.Services;

public class AuthService : IAuthService
{
    private const int MaxRegistrationCodeAttempts = 3;
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IEmailService _emailService;
    private readonly IMemoryCache _memoryCache;

    public AuthService(
        IUserRepository userRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        IPasswordHasher<User> passwordHasher,
        IEmailService emailService,
        IMemoryCache memoryCache)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
        _memoryCache = memoryCache;
    }

    public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default)
    {
        var modelCheck = ModelValidator.Validate(dto);
        if (!modelCheck.IsSuccess)
        {
            return Result<AuthResponseDto>.Failure(modelCheck.Error!, modelCheck.ErrorType);
        }

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        if (await _userRepository.EmailExistsAsync(normalizedEmail, null, cancellationToken))
        {
            return Result<AuthResponseDto>.Failure("Пользователь с таким Email уже зарегистрирован.", ErrorType.Conflict);
        }

        var user = new User
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = normalizedEmail,
            Role = UserRole.Student,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

        await _userRepository.AddAsync(user, cancellationToken);

        var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(user);

        return Result<AuthResponseDto>.Success(new AuthResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = MapToUserDto(user)
        });
    }

    public async Task<Result> StartRegistrationAsync(RegisterDto dto, CancellationToken cancellationToken = default)
    {
        var modelCheck = ModelValidator.Validate(dto);
        if (!modelCheck.IsSuccess)
        {
            return modelCheck;
        }

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        if (await _userRepository.EmailExistsAsync(normalizedEmail, null, cancellationToken))
        {
            return Result.Failure("Пользователь с таким Email уже зарегистрирован.", ErrorType.Conflict);
        }

        var tempUser = new User
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = normalizedEmail
        };

        var code = Random.Shared.Next(100000, 999999).ToString();
        var pendingState = new PendingRegistrationState
        {
            FirstName = tempUser.FirstName,
            LastName = tempUser.LastName,
            Email = normalizedEmail,
            PasswordHash = _passwordHasher.HashPassword(tempUser, dto.Password),
            Code = code,
            FailedAttempts = 0
        };

        _memoryCache.Set(GetRegistrationCacheKey(normalizedEmail), pendingState, TimeSpan.FromMinutes(15));

        return await SendRegistrationCodeEmailAsync(pendingState.Email, pendingState.FirstName, code);
    }

    public Result<VerifyRegistrationDto> GetPendingRegistrationState(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result<VerifyRegistrationDto>.Failure("Сначала заполните форму регистрации.", ErrorType.Validation);
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();
        if (!_memoryCache.TryGetValue(GetRegistrationCacheKey(normalizedEmail), out PendingRegistrationState? state) || state is null)
        {
            return Result<VerifyRegistrationDto>.Failure("Сессия регистрации истекла или не найдена. Пожалуйста, заполните форму заново.", ErrorType.NotFound);
        }

        return Result<VerifyRegistrationDto>.Success(new VerifyRegistrationDto
        {
            Email = state.Email,
            FailedAttempts = state.FailedAttempts
        });
    }

    public async Task<Result<AuthResponseDto>> VerifyRegistrationCodeAsync(VerifyRegistrationDto dto, CancellationToken cancellationToken = default)
    {
        var modelCheck = ModelValidator.Validate(dto);
        if (!modelCheck.IsSuccess)
        {
            return Result<AuthResponseDto>.Failure(modelCheck.Error!, modelCheck.ErrorType);
        }

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var cacheKey = GetRegistrationCacheKey(normalizedEmail);

        if (!_memoryCache.TryGetValue(cacheKey, out PendingRegistrationState? state) || state is null)
        {
            return Result<AuthResponseDto>.Failure("Время ожидания подтверждения истекло. Пройдите регистрацию заново.", ErrorType.BadRequest);
        }

        if (state.FailedAttempts >= MaxRegistrationCodeAttempts)
        {
            dto.FailedAttempts = state.FailedAttempts;
            return Result<AuthResponseDto>.Failure(
                "Вы 3 раза ввели неверный код. Нажмите кнопку «Отправить новый код», чтобы получить новый код на Email.",
                ErrorType.Validation);
        }

        if (!string.Equals(state.Code.Trim(), dto.Code.Trim(), StringComparison.Ordinal))
        {
            state.FailedAttempts++;
            _memoryCache.Set(cacheKey, state, TimeSpan.FromMinutes(15));
            dto.FailedAttempts = state.FailedAttempts;
            dto.Code = string.Empty;

            if (state.FailedAttempts >= MaxRegistrationCodeAttempts)
            {
                return Result<AuthResponseDto>.Failure(
                    "Вы 3 раза ввели неверный код. Нажмите кнопку «Отправить новый код», чтобы получить новый код на Email.",
                    ErrorType.Validation);
            }

            var remaining = MaxRegistrationCodeAttempts - state.FailedAttempts;
            return Result<AuthResponseDto>.Failure(
                $"Неверный код! Пожалуйста, введите код ещё раз (осталось попыток: {remaining}).",
                ErrorType.Validation);
        }

        if (await _userRepository.EmailExistsAsync(normalizedEmail, null, cancellationToken))
        {
            _memoryCache.Remove(cacheKey);
            return Result<AuthResponseDto>.Failure("Пользователь с таким Email уже зарегистрирован.", ErrorType.Conflict);
        }

        var user = new User
        {
            FirstName = state.FirstName,
            LastName = state.LastName,
            Email = state.Email,
            PasswordHash = state.PasswordHash,
            Role = UserRole.Student,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user, cancellationToken);
        _memoryCache.Remove(cacheKey);

        var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(user);

        return Result<AuthResponseDto>.Success(new AuthResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = MapToUserDto(user)
        });
    }

    public async Task<Result> ResendRegistrationCodeAsync(string? email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure("Email не указан.", ErrorType.Validation);
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var cacheKey = GetRegistrationCacheKey(normalizedEmail);

        if (!_memoryCache.TryGetValue(cacheKey, out PendingRegistrationState? state) || state is null)
        {
            return Result.Failure("Сессия регистрации истекла. Заполните форму регистрации заново.", ErrorType.NotFound);
        }

        state.Code = Random.Shared.Next(100000, 999999).ToString();
        state.FailedAttempts = 0;

        _memoryCache.Set(cacheKey, state, TimeSpan.FromMinutes(15));

        return await SendRegistrationCodeEmailAsync(state.Email, state.FirstName, state.Code);
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        var modelCheck = ModelValidator.Validate(dto);
        if (!modelCheck.IsSuccess)
        {
            return Result<AuthResponseDto>.Failure(modelCheck.Error!, modelCheck.ErrorType);
        }

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null)
        {
            return Result<AuthResponseDto>.Failure("Неверный Email или пароль.", ErrorType.Unauthorized);
        }

        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return Result<AuthResponseDto>.Failure("Неверный Email или пароль.", ErrorType.Unauthorized);
        }

        if (!user.IsActive)
        {
            return Result<AuthResponseDto>.Failure("Ваш аккаунт заблокирован администратором. Вход в систему невозможен.", ErrorType.Forbidden);
        }

        var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(user);

        return Result<AuthResponseDto>.Success(new AuthResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = MapToUserDto(user)
        });
    }

    public async Task<Result<UserDto>> GetCurrentUserAsync(int? userId, CancellationToken cancellationToken = default)
    {
        if (!userId.HasValue || userId.Value <= 0)
        {
            return Result<UserDto>.Failure("Пользователь не авторизован.", ErrorType.Unauthorized);
        }

        var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
        if (user is null)
        {
            return Result<UserDto>.Failure("Пользователь не найден.", ErrorType.NotFound);
        }

        if (!user.IsActive)
        {
            return Result<UserDto>.Failure("Аккаунт пользователя заблокирован.", ErrorType.Forbidden);
        }

        return Result<UserDto>.Success(MapToUserDto(user));
    }

    public async Task<Result> ForgotPasswordAsync(ForgotPasswordDto dto, CancellationToken cancellationToken = default)
    {
        var modelCheck = ModelValidator.Validate(dto);
        if (!modelCheck.IsSuccess)
        {
            return modelCheck;
        }

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null)
        {
            return Result.Failure("Пользователь с таким Email не найден.", ErrorType.NotFound);
        }

        if (!user.IsActive)
        {
            return Result.Failure("Ваш аккаунт заблокирован администратором.", ErrorType.Forbidden);
        }

        var code = Random.Shared.Next(100000, 999999).ToString();
        var cacheKey = GetResetCacheKey(normalizedEmail);

        _memoryCache.Set(cacheKey, code, TimeSpan.FromMinutes(15));

        var subject = "CourseHub — Код для восстановления пароля";
        var body = $@"
            <div style='font-family: Arial, sans-serif; max-width: 520px; margin: 0 auto; padding: 24px; border: 1px solid #e2e8f0; border-radius: 12px;'>
                <h2 style='color: #1e1b4b; margin-top: 0;'>Восстановление пароля CourseHub</h2>
                <p>Здравствуйте, <strong>{user.FirstName}</strong>!</p>
                <p>Вы запросили сброс пароля для вашей учётной записи на платформе <strong>CourseHub</strong>.</p>
                <p>Ваш одноразовый код подтверждения:</p>
                <div style='font-size: 28px; font-weight: bold; letter-spacing: 6px; color: #4f46e5; background: #eef2ff; padding: 14px 20px; border-radius: 8px; text-align: center; margin: 20px 0;'>
                    {code}
                </div>
                <p style='color: #64748b; font-size: 13px;'>Код действителен в течение 15 минут. Если вы не запрашивали восстановление пароля, просто проигнорируйте это письмо.</p>
            </div>";

        try
        {
            await _emailService.SendAsync(user.Email, subject, body);
        }
        catch (Exception)
        {
            return Result.Failure("Не удалось отправить письмо с кодом. Проверьте настройки SMTP в appsettings.json.", ErrorType.BadRequest);
        }

        return Result.Success();
    }

    public async Task<Result> VerifyResetCodeAsync(VerifyResetCodeDto dto, CancellationToken cancellationToken = default)
    {
        var modelCheck = ModelValidator.Validate(dto);
        if (!modelCheck.IsSuccess)
        {
            return modelCheck;
        }

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null)
        {
            return Result.Failure("Пользователь с таким Email не найден.", ErrorType.NotFound);
        }

        var cacheKey = GetResetCacheKey(normalizedEmail);
        if (!_memoryCache.TryGetValue(cacheKey, out string? savedCode) || string.IsNullOrWhiteSpace(savedCode))
        {
            return Result.Failure("Срок действия кода истёк или код не запрашивался. Запросите новый код.", ErrorType.BadRequest);
        }

        if (!string.Equals(savedCode.Trim(), dto.Code.Trim(), StringComparison.Ordinal))
        {
            return Result.Failure("Неверный код подтверждения. Проверьте письмо и попробуйте снова.", ErrorType.BadRequest);
        }

        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordDto dto, CancellationToken cancellationToken = default)
    {
        var modelCheck = ModelValidator.Validate(dto);
        if (!modelCheck.IsSuccess)
        {
            return modelCheck;
        }

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null)
        {
            return Result.Failure("Пользователь с таким Email не найден.", ErrorType.NotFound);
        }

        if (!user.IsActive)
        {
            return Result.Failure("Аккаунт заблокирован.", ErrorType.Forbidden);
        }

        var cacheKey = GetResetCacheKey(normalizedEmail);
        if (!_memoryCache.TryGetValue(cacheKey, out string? savedCode) || string.IsNullOrWhiteSpace(savedCode))
        {
            return Result.Failure("Срок действия кода истёк. Запросите новый код.", ErrorType.BadRequest);
        }

        if (!string.Equals(savedCode.Trim(), dto.Code.Trim(), StringComparison.Ordinal))
        {
            return Result.Failure("Неверный код подтверждения.", ErrorType.BadRequest);
        }

        user.PasswordHash = _passwordHasher.HashPassword(user, dto.NewPassword);
        await _userRepository.UpdateAsync(user, cancellationToken);

        _memoryCache.Remove(cacheKey);

        return Result.Success();
    }

    private async Task<Result> SendRegistrationCodeEmailAsync(string email, string firstName, string code)
    {
        var subject = "CourseHub — Код подтверждения регистрации";
        var body = $@"
            <div style='font-family: Arial, sans-serif; max-width: 520px; margin: 0 auto; padding: 24px; border: 1px solid #e2e8f0; border-radius: 12px;'>
                <h2 style='color: #1e1b4b; margin-top: 0;'>Подтверждение регистрации на CourseHub</h2>
                <p>Здравствуйте, <strong>{firstName}</strong>!</p>
                <p>Для завершения создания аккаунта студента введите этот код подтверждения на сайте:</p>
                <div style='font-size: 28px; font-weight: bold; letter-spacing: 6px; color: #4f46e5; background: #eef2ff; padding: 14px 20px; border-radius: 8px; text-align: center; margin: 20px 0;'>
                    {code}
                </div>
                <p style='color: #64748b; font-size: 13px;'>У вас есть 3 попытки ввода кода. Код действителен в течение 15 минут.</p>
            </div>";

        try
        {
            await _emailService.SendAsync(email, subject, body);
            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure("Не удалось отправить код на указанный Email. Проверьте настройки SMTP.", ErrorType.BadRequest);
        }
    }

    private static string GetResetCacheKey(string normalizedEmail) => $"pwd_reset_code:{normalizedEmail}";
    private static string GetRegistrationCacheKey(string normalizedEmail) => $"reg_verify_code:{normalizedEmail}";

    private static UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            EnrolledCoursesCount = user.Enrollments?.Count ?? 0
        };
    }
}
