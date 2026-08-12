using UserService.Application.Exceptions;

namespace UserService.Api.Middlewares;

public class GlobalExceptionHandlingMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            Console.WriteLine("Before next");
            await next(context);
            Console.WriteLine("After next");
        }
        catch (Exception e)
        {
            context.Response.ContentType = "application/json";
            if (e is BaseException baseException)
            {
                context.Response.StatusCode = baseException.StatusCode;

                await context.Response.WriteAsJsonAsync(new
                {
                    status = context.Response.StatusCode,
                    message = baseException.Message
                });
            }
            
            else
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "Internal server error"
                });
            }
        }
    }
}