using FluentValidation.Results;

namespace Reevon.Api.Contracts.Response;

public sealed class ApiError
{
    public int Code { get; set; }
    public string Message { get; set; } = "";
    public Dictionary<string, List<string>> ValidationErrors { get; set; } = new();

    public static ApiError FromValidation(ValidationResult result)
    {
        var error = new ApiError
        {
            Code = 400,
            Message = "Validation error",
        };

        foreach (ValidationFailure errorItem in result.Errors)
        {
            string key = errorItem.PropertyName;
            string message = errorItem.ErrorMessage;
            if (error.ValidationErrors.TryGetValue(key, out var validationError))
            {
                validationError.Add(message);
            }
            else
            {
                error.ValidationErrors.Add(key, new List<string> { message });
            }
        }

        return error;
    }

    public static ApiError FromString(string error)
    {
        var apiError = new ApiError
        {
            Code = 400,
            Message = error,
        };
        return apiError;
    }
}