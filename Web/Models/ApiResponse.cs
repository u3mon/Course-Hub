using Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace Web.Models;

public class ApiResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }

    public static ApiResponse Ok(string? message = null) => new()
    {
        Success = true,
        Message = message
    };

    public static ApiResponse Fail(string error, List<string>? errors = null) => new()
    {
        Success = false,
        Message = error,
        Errors = errors
    };
}

public class ApiResponse<T> : ApiResponse
{
    public T? Data { get; set; }

    public static ApiResponse<T> Ok(T data, string? message = null) => new()
    {
        Success = true,
        Data = data,
        Message = message
    };

    public new static ApiResponse<T> Fail(string error, List<string>? errors = null) => new()
    {
        Success = false,
        Message = error,
        Errors = errors
    };
}

public static class ResultActionResultExtensions
{
    public static IActionResult ToActionResult(this Result result, string? successMessage = null)
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(ApiResponse.Ok(successMessage));
        }

        return CreateErrorResult(result.Error ?? "Произошла ошибка.", result.ErrorType);
    }

    public static IActionResult ToActionResult<T>(this Result<T> result, string? successMessage = null, int successStatusCode = 200)
    {
        if (result.IsSuccess)
        {
            var response = ApiResponse<T>.Ok(result.Value!, successMessage);
            return new ObjectResult(response) { StatusCode = successStatusCode };
        }

        return CreateErrorResult(result.Error ?? "Произошла ошибка.", result.ErrorType);
    }

    private static IActionResult CreateErrorResult(string error, ErrorType errorType)
    {
        var statusCode = errorType switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.BadRequest => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest
        };

        return new ObjectResult(ApiResponse.Fail(error))
        {
            StatusCode = statusCode
        };
    }
}
