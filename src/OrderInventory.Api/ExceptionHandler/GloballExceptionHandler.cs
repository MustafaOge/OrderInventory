using System;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderInventory.Application.Exceptions;

namespace OrderInventory.Api.ExceptionHandler;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
            BadRequestException => (StatusCodes.Status400BadRequest, "Bad Request"),
            NegativeStockException => (StatusCodes.Status400BadRequest, "Bad Request"),
            InsufficientStockException => (StatusCodes.Status409Conflict, "Conflict"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
            DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Concurrency Conflict"),
            _ => (0, string.Empty)
        };

        if (statusCode == 0)
            return false;

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message
        }, cancellationToken);

        return true;
    }
}
