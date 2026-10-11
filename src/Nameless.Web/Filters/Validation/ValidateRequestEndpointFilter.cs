using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Nameless.Web.ObjectModel;

namespace Nameless.Web.Filters.Validation;

/// <summary>
///     Endpoint filter that provides validation capabilities.
/// </summary>
public class ValidateRequestEndpointFilter : ValidationFilterBase, IEndpointFilter {
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next) {
        var disableValidation = context.HttpContext.GetEndpoint()?.Metadata.GetMetadata<IDisableValidationMetadata>();
        if (disableValidation is not null) {
            return await next(context);
        }

        var result = await ValidateRequestObjectsAsync(
            context.HttpContext.RequestServices,
            context.Arguments,
            context.HttpContext.RequestAborted
        );

        return await result.Match(
            onSuccess: _ => next(context),
            onFailure: errors => ValueTask.FromResult<object?>(
                TypedResults.ValidationProblem(errors.ToDictionary())
            )
        );
    }
}