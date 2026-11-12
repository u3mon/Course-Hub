using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth;

public class RegisterDto
{
    [Required(ErrorMessage = "Имя обязательно для заполнения")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Имя должно быть от 2 до 50 символов")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Фамилия обязательна для заполнения")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Фамилия должна быть от 2 до 50 символов")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email обязателен для заполнения")]
    [EmailAddress(ErrorMessage = "Введите корректный адрес электронной почты")]
    [StringLength(100, ErrorMessage = "Email не должен превышать 100 символов")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Пароль обязателен для заполнения")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен содержать минимум 6 символов")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Подтверждение пароля обязательно")]
    [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class PendingRegistrationState
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int FailedAttempts { get; set; }
}

public class VerifyRegistrationDto
{
    [Required(ErrorMessage = "Email обязателен")]
    [EmailAddress(ErrorMessage = "Введите корректный формат Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите код подтверждения из письма")]
    [StringLength(10, MinimumLength = 4, ErrorMessage = "Введите 6-значный код из письма")]
    public string Code { get; set; } = string.Empty;

    public int FailedAttempts { get; set; }
    public int RemainingAttempts => Math.Max(0, 3 - FailedAttempts);
    public bool CanResendCode => FailedAttempts >= 3;
}

public class LoginDto
{
    [Required(ErrorMessage = "Введите Email")]
    [EmailAddress(ErrorMessage = "Некорректный формат Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите пароль")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}

public class ForgotPasswordDto
{
    [Required(ErrorMessage = "Введите Email вашего аккаунта")]
    [EmailAddress(ErrorMessage = "Введите корректный формат Email")]
    public string Email { get; set; } = string.Empty;
}

public class VerifyResetCodeDto
{
    [Required(ErrorMessage = "Email обязателен")]
    [EmailAddress(ErrorMessage = "Введите корректный формат Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите код, который пришёл на ваш Email")]
    [StringLength(10, MinimumLength = 4, ErrorMessage = "Введите 6-значный код из письма")]
    public string Code { get; set; } = string.Empty;
}

public class ResetPasswordDto
{
    [Required(ErrorMessage = "Email обязателен")]
    [EmailAddress(ErrorMessage = "Введите корректный формат Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Код подтверждения обязателен")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите новый пароль")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Новый пароль должен содержать минимум 6 символов")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Подтвердите новый пароль")]
    [Compare(nameof(NewPassword), ErrorMessage = "Пароли не совпадают")]
    [DataType(DataType.Password)]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}

public class UserDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int EnrolledCoursesCount { get; set; }
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = new();
}
