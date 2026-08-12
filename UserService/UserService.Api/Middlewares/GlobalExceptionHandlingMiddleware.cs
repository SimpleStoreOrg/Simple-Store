using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.Exceptions;

namespace UserService.Api.Middlewares;

public class GlobalExceptionHandlingMiddleware : IMiddleware
{
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
    public GlobalExceptionHandlingMiddleware(ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _logger = logger;
    }
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            
            context.Response.ContentType = MediaTypeNames.Application.Json;
            
            if (e is BaseException baseException)
            {
                context.Response.StatusCode = baseException.StatusCode;

                var problem = new ProblemDetails
                {
                    Status = baseException.StatusCode,
                    Title = "Server error",
                    Detail = baseException.Message,
                    Instance = context.Request.Path
                };
                
                await context.Response.WriteAsJsonAsync(problem);
            }
            
            else
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                
                var problem = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Detail = "An internal server error has occured",
                    Instance = context.Request.Path
                };

                await context.Response.WriteAsJsonAsync(problem);
            }
        }
    }
}