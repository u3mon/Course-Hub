using Application.Common;
using Application.DTOs.Auth;
using Microsoft.AspNetCore.Mvc;
using Web.Controllers.Api;

namespace Web.Extensions;

public static class ControllerResultExtensions
{
    public static Result<AuthResponseDto> WithAuthCookie(
        this Result<AuthResponseDto> result,
        HttpResponse response,
        bool isHttps)
    {
        if (result.IsSuccess && result.Value is not null)
        {
            response.Cookies.Append(AuthApiController.AuthCookieName, result.Value.Token, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                Secure = isHttps,
                Expires = result.Value.ExpiresAt
            });
        }

        return result;
    }

    public static IActionResult ToRedirectResult(
        this Controller controller,
        Result result,
        string successMessage,
        string fallbackAction,
        string? fallbackController = null,
        object? routeValues = null,
        string? returnUrl = null,
        string successKey = "Success")
    {
        if (result.IsSuccess)
        {
            controller.TempData[successKey] = successMessage;
        }
        else
        {
            controller.TempData["Error"] = result.Error ?? "Произошла ошибка при выполнении операции.";
        }

        if (!string.IsNullOrWhiteSpace(returnUrl) && controller.Url.IsLocalUrl(returnUrl))
        {
            return controller.Redirect(returnUrl);
        }

        return fallbackController is not null
            ? controller.RedirectToAction(fallbackAction, fallbackController, routeValues)
            : controller.RedirectToAction(fallbackAction, routeValues);
    }

    public static IActionResult ToViewOrRedirect<T>(
        this Controller controller,
        Result<T> result,
        string fallbackAction,
        string? fallbackController = null)
    {
        if (!result.IsSuccess || result.Value is null)
        {
            controller.TempData["Error"] = result.Error ?? "Запрошенные данные не найдены.";
            return fallbackController is not null
                ? controller.RedirectToAction(fallbackAction, fallbackController)
                : controller.RedirectToAction(fallbackAction);
        }

        return controller.View(result.Value);
    }

    public static IActionResult ToFormResult(
        this Controller controller,
        Result result,
        object viewModel,
        string successMessage,
        string redirectAction,
        string? redirectController = null,
        object? routeValues = null)
    {
        if (!result.IsSuccess)
        {
            controller.ModelState.Clear();
            controller.ModelState.AddModelError(string.Empty, result.Error ?? "Ошибка проверки данных.");
            return controller.View(viewModel);
        }

        controller.TempData["Success"] = successMessage;
        return redirectController is not null
            ? controller.RedirectToAction(redirectAction, redirectController, routeValues)
            : controller.RedirectToAction(redirectAction, routeValues);
    }

    public static IActionResult ToAuthFormResult(
        this Controller controller,
        Result<AuthResponseDto> result,
        object viewModel,
        string? returnUrl = null,
        string? customSuccessMessage = null)
    {
        if (!result.IsSuccess || result.Value is null)
        {
            controller.ModelState.Clear();
            controller.ModelState.AddModelError(string.Empty, result.Error ?? "Ошибка авторизации.");
            return controller.View(viewModel);
        }

        controller.WithAuthCookie(result);
        controller.TempData["Success"] = customSuccessMessage ?? $"Добро пожаловать, {result.Value.User.FirstName}!";

        if (!string.IsNullOrWhiteSpace(returnUrl) && controller.Url.IsLocalUrl(returnUrl))
        {
            return controller.Redirect(returnUrl);
        }

        return result.Value.User.Role == "Admin"
            ? controller.RedirectToAction("Index", "Admin")
            : controller.RedirectToAction("Index", "Courses");
    }

    private static void WithAuthCookie(this Controller controller, Result<AuthResponseDto> result)
    {
        result.WithAuthCookie(controller.Response, controller.Request.IsHttps);
    }
}
