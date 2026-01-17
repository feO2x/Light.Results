using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Light.Results.AspNetCore.Shared.HttpContextInjection;

public sealed class HttpContextInjectionMiddleware
{
    private readonly RequestDelegate _next;

    public HttpContextInjectionMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var scopedServicesWithNeedForHttpContext = context.RequestServices.GetServices<IInjectHttpContext>();
        switch (scopedServicesWithNeedForHttpContext)
        {
            // Microsoft.Extensions.DependencyInjection returns an array internally, thus we can avoid the
            // Enumerator allocation if it is possible to cast.
            case IInjectHttpContext[] array:
                foreach (var scopedService in array)
                {
                    scopedService.HttpContext = context;
                }

                break;

            // The user is likely using another DI container adapted to IServiceProvider, and it does not return
            // an array when calling GetServices. We simply fall back to enumerate over IEnumerable<T> - but this
            // allocates.
            default:
                foreach (var scopedService in scopedServicesWithNeedForHttpContext)
                {
                    scopedService.HttpContext = context;
                }

                break;
        }

        await _next(context);
    }
}
