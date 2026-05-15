using System.Net;
using System.Text.Json;

namespace RestaurantReservation.API.Middleware
{
    // Middleware wraps every HTTP request in a TRY / CATCH
    // If anything crashes anywhere in the app, this catches it
    // and returns it as a clean JSON error instead of a exception
    public class ExceptionMiddleware
    {
        // RequestDelegate equals to the next middleware in the pipeline
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next   = next;
            _logger = logger;
        }

        // InvokeAsync is called on every HTTP request
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Try to pass the request to the next middleware
                await _next(context);
            }
            catch (Exception ex)
            {
                // Something crashed - log it and return clean error
                _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            // Determine the status code based on exception type
            var statusCode = ex switch
            {
                // 404 - resource not found
                KeyNotFoundException     => HttpStatusCode.NotFound,
                // 400 - bad request (invalid input)
                ArgumentException        => HttpStatusCode.BadRequest,
                // 401 - unauthorized
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                // 500 - everything else is a server error
                _                        => HttpStatusCode.InternalServerError
            };

            // Build a clean error response object
            var response = new
            {
                statusCode = (int)statusCode,
                message    = ex switch
                {
                    KeyNotFoundException     => ex.Message,
                    ArgumentException        => ex.Message,
                    UnauthorizedAccessException => "You are not authorized.",
                    // Never expose internal error details in production
                    _                        => "An unexpected error occurred."
                },
                // Only show details in development
                details = context.RequestServices
                    .GetRequiredService<IHostEnvironment>()
                    .IsDevelopment() ? ex.StackTrace : null
            };

            // Set response type to JSON
            context.Response.ContentType = "application/json";
            context.Response.StatusCode  = (int)statusCode;

            // Serialize and write the response
            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}