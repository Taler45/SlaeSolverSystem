using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SlaeSolver.Domain.Exceptions;

namespace SlaeSolver.API.Infrastructure;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        (int status, string title) result;
        switch (exception)
        {
            case DomainException:
                result = (StatusCodes.Status400BadRequest, "Некоректні дані");
                break;

            case InvalidCredentialsException:
                result = (StatusCodes.Status401Unauthorized, "Не авторизовано");
                break;

            case NotFoundException:
                result = (StatusCodes.Status404NotFound, "Не знайдено");
                break;

            case ConflictException:
                result = (StatusCodes.Status409Conflict, "Конфлікт");
                break;

            case LimitExceededException:
                result = (StatusCodes.Status422UnprocessableEntity, "Перевищено ліміт");
                break;

            default:
                result = (StatusCodes.Status500InternalServerError, "Внутрішня помилка сервера");
                break;
        }
        
        if (result.status == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Необроблений виняток");
        }
        
        var problem = new ProblemDetails
        {
            Status = result.status,
            Title = result.title,

            Detail = result.status == StatusCodes.Status500InternalServerError ? null : exception.Message
        };
        
        httpContext.Response.StatusCode = result.status;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        
        return true;
    }
}