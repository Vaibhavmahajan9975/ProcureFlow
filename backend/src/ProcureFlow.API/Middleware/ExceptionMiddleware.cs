using FluentValidation;
using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using ProcureFlow.Application.Common;
namespace ProcureFlow.API.Middleware;
public class ExceptionMiddleware(RequestDelegate next,ILogger<ExceptionMiddleware> logger, IWebHostEnvironment env)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch(Exception ex)
        {
            logger.LogError(ex,"Unhandled request error");
            context.Response.ContentType = "application/json";
            var (code, msg, errors) = ex switch { ValidationException v => (400, "Validation failed.", v.Errors.Select(x => x.ErrorMessage).ToArray()), NotFoundException n => (404, n.Message, Array.Empty<string>()), BusinessRuleException b => (409, b.Message, Array.Empty<string>()), UnauthorizedAccessException u => (403, u.Message, Array.Empty<string>()), _ => (500, "An unexpected error occurred.", Array.Empty<string>()) };
            context.Response.StatusCode = code;
            // Do not expose stack traces or exception details to the client. Keep response shape stable.
            await context.Response.WriteAsJsonAsync(new { statusCode = code, message = msg, errors });
        }
    }
}