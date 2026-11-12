using System.ComponentModel.DataAnnotations;

namespace Application.Common;

public static class ModelValidator
{
    public static Result Validate<T>(T? model)
    {
        if (model is null)
        {
            return Result.Failure("Не переданы входные данные.", ErrorType.Validation);
        }

        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        if (!Validator.TryValidateObject(model, context, results, validateAllProperties: true))
        {
            var firstError = results.FirstOrDefault()?.ErrorMessage ?? "Ошибка валидации входных данных.";
            return Result.Failure(firstError, ErrorType.Validation);
        }

        return Result.Success();
    }
}
