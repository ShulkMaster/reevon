using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Reevon.Api.Contracts.Response;

namespace Reevon.Api.Setup;

public static class ErrorSetUp
{
    public static void SetUpErrors(this IMvcBuilder mvc)
    {
        mvc.ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = ErrorResult;
        });
    }

    private static IActionResult ErrorResult(ActionContext context)
    {
        var keys = context.ModelState.Keys;
        var error = new ApiError
        {
            Code = 400,
            Message = "Validation error",
        };

        foreach (string key in keys)
        {
            if (context.ModelState[key].Errors.Count < 1) continue;
            ModelErrorCollection errors = context.ModelState[key]!.Errors;
            var errorMessages = errors.Select(x => x.ErrorMessage);
            error.ValidationErrors.Add(key, errorMessages.ToList());
        }
        return new BadRequestObjectResult(error);
    }
}