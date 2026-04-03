using Ecommerce.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Serilog.Debugging;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Ecommerce.API.Handlers
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var (statusCode, message, error) = GetExceptionDetails(exception);

            _logger.LogError(exception, "An error occurred: {Message}", exception.Message);

            httpContext.Response.StatusCode = (int)statusCode;
            await httpContext.Response.WriteAsJsonAsync(new
            {
                Success = false,
                Message = message,
                Errors = error
            }, cancellationToken);

            return true;
        }

        private (HttpStatusCode statusCode, string message, object? errors) GetExceptionDetails(Exception exception)
        {
            return exception switch
            {

                ValidationException valEx => (HttpStatusCode.BadRequest, "Invalid input data.", valEx.Errors),
                NotFoundException nfEx => (HttpStatusCode.NotFound, nfEx.Message, null),
                BadRequestException badEx => (HttpStatusCode.BadRequest, badEx.Message, null),
                UnauthorizedException unAuthEx => (HttpStatusCode.Unauthorized, unAuthEx.Message, null),
                ForbiddenException badEx => (HttpStatusCode.BadRequest, badEx.Message, null),

                LoginFailedException => (HttpStatusCode.Unauthorized, exception.Message, null),
                UserAlreadyExistsException => (HttpStatusCode.Conflict, exception.Message, null),
                RegistrationFailedException => (HttpStatusCode.BadRequest, exception.Message, null),
                RefreshTokenException => (HttpStatusCode.Unauthorized, exception.Message, null),
                _ => (HttpStatusCode.InternalServerError, $"An unexpected error occurred.", null)
            };
        }
    }
}
