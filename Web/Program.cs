using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Application;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Infrastructure;
using Infrastructure.Authentication;
using Infrastructure.Data;
using Infrastructure.Email;
using Infrastructure.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Web.Controllers.Api;
using Web.Middleware;
using Web.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();

var emailSettings = builder.Configuration.GetSection("Email").Get<EmailSettings>() ?? new EmailSettings();
builder.Services.AddSingleton(emailSettings);
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<IFileStorageService>(_ => new FileStorageService(builder.Environment.WebRootPath));

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllersWithViews()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            var response = ApiResponse.Fail(
                errors.FirstOrDefault() ?? "Ошибка валидации входных данных.",
                errors);

            return new BadRequestObjectResult(response);
        };
    });

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("JwtSettings section is missing in configuration.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.Name
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(authHeader) &&
                context.Request.Cookies.TryGetValue(AuthApiController.AuthCookieName, out var cookieToken) &&
                !string.IsNullOrWhiteSpace(cookieToken))
            {
                context.Token = cookieToken;
            }
            return Task.CompletedTask;
        },
        OnTokenValidated = async context =>
        {
            var userIdStr = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier)
                            ?? context.Principal?.FindFirstValue("sub");

            if (!int.TryParse(userIdStr, out var userId))
            {
                context.Fail("Некорректный идентификатор пользователя в токене.");
                return;
            }

            var userRepository = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
            var user = await userRepository.GetByIdAsync(userId, context.HttpContext.RequestAborted);

            if (user is null || !user.IsActive)
            {
                context.HttpContext.Response.Cookies.Delete(AuthApiController.AuthCookieName);
                context.Fail("Аккаунт пользователя заблокирован или удалён.");
            }
        },
        OnChallenge = async context =>
        {
            context.HandleResponse();

            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json; charset=utf-8";
                var payload = JsonSerializer.Serialize(
                    ApiResponse.Fail("Требуется авторизация. Передайте действительный JWT-токен."),
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                await context.Response.WriteAsync(payload);
            }
            else
            {
                var returnUrl = Uri.EscapeDataString(context.Request.Path + context.Request.QueryString);
                context.Response.Redirect($"/Account/Login?returnUrl={returnUrl}");
            }
        },
        OnForbidden = async context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json; charset=utf-8";
                var payload = JsonSerializer.Serialize(
                    ApiResponse.Fail("Доступ запрещён. У вас недостаточно прав для выполнения этой операции."),
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                await context.Response.WriteAsync(payload);
            }
            else
            {
                context.Response.Redirect("/Account/AccessDenied");
            }
        }
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

await DbSeeder.SeedAsync(app.Services);

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Courses}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
