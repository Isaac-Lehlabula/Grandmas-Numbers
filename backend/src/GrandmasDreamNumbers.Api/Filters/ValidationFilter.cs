using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GrandmasDreamNumbers.Api.Filters;

/// <summary>
/// Runs any registered FluentValidation validator against the incoming
/// action arguments before the action executes, so controllers don't have
/// to call validators themselves.
/// </summary>
public class ValidationFilter(IServiceProvider serviceProvider) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
            {
                continue;
            }

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (serviceProvider.GetService(validatorType) is not IValidator validator)
            {
                continue;
            }

            var validationContext = new ValidationContext<object>(argument);
            var result = await validator.ValidateAsync(validationContext);

            if (!result.IsValid)
            {
                var errors = result.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                context.Result = new BadRequestObjectResult(new ValidationProblemDetails(errors));
                return;
            }
        }

        await next();
    }
}
