using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TaskManagement.API.Filters;

/// <summary>Action filter that runs FluentValidation and short-circuits with 400 on failure.</summary>
public sealed class ValidationFilter<T>(IValidator<T> validator) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var model = context.ActionArguments.Values.OfType<T>().FirstOrDefault();
        if (model is null)
        {
            await next();
            return;
        }

        var result = await validator.ValidateAsync(model);
        if (!result.IsValid)
        {
            var errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            context.Result = new BadRequestObjectResult(new ValidationProblemDetails(errors));
            return;
        }

        await next();
    }
}
