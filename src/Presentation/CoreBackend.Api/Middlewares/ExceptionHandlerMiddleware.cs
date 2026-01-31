using System.Net;
using System.Text.Json;
using CoreBackend.Domain.Errors;
using CoreBackend.Domain.Exceptions;
using UnauthorizedAccessException = CoreBackend.Domain.Exceptions.UnauthorizedAccessException;

namespace CoreBackend.Api.Middlewares;

/// <summary>
/// Global exception handler middleware.
/// Tüm hataları yakalar ve tutarlı format döner.
/// </summary>
public class ExceptionHandlerMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<ExceptionHandlerMiddleware> _logger;

	public ExceptionHandlerMiddleware(
		RequestDelegate next,
		ILogger<ExceptionHandlerMiddleware> logger)
	{
		_next = next;
		_logger = logger;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await _next(context);
		}
		catch (Exception ex)
		{
			await HandleExceptionAsync(context, ex);
		}
	}

	private async Task HandleExceptionAsync(HttpContext context, Exception exception)
	{
		var (statusCode, errorResponse) = exception switch
		{
			ValidationException validationEx => (
				HttpStatusCode.BadRequest,
				CreateValidationErrorResponse(validationEx)),

			NotFoundException notFoundEx => (
				HttpStatusCode.NotFound,
				CreateErrorResponse(notFoundEx.Error)),

			UnauthorizedAccessException unauthorizedEx => (
				HttpStatusCode.Unauthorized,
				CreateErrorResponse(unauthorizedEx.Error)),

			ConflictException conflictEx => (
				HttpStatusCode.Conflict,
				CreateErrorResponse(conflictEx.Error)),

			DomainException domainEx => (
				HttpStatusCode.BadRequest,
				CreateErrorResponse(domainEx.Error)),

			_ => (
				HttpStatusCode.InternalServerError,
				CreateGenericErrorResponse())
		};

		// Loglama
		LogException(exception, statusCode);

		// Response yaz
		context.Response.ContentType = "application/json";
		context.Response.StatusCode = (int)statusCode;

		var jsonOptions = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};

		await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, jsonOptions));
	}

	private void LogException(Exception exception, HttpStatusCode statusCode)
	{
		if (statusCode == HttpStatusCode.InternalServerError)
		{
			_logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
		}
		else
		{
			_logger.LogWarning("Handled exception: {ExceptionType} - {Message}",
				exception.GetType().Name, exception.Message);
		}
	}

	private static ErrorResponse CreateErrorResponse(Error error)
	{
		return new ErrorResponse
		{
			Success = false,
			Error = new ErrorDetail
			{
				Code = error.Code,
				Message = error.Message
			}
		};
	}

	private static ErrorResponse CreateValidationErrorResponse(ValidationException exception)
	{
		return new ErrorResponse
		{
			Success = false,
			Error = new ErrorDetail
			{
				Code = ErrorCodes.General.ValidationError,
				Message = "One or more validation errors occurred.",
				Details = (Dictionary<string, string[]>)exception.Errors
			}
		};
	}

	private static ErrorResponse CreateGenericErrorResponse()
	{
		return new ErrorResponse
		{
			Success = false,
			Error = new ErrorDetail
			{
				Code = ErrorCodes.General.UnexpectedError,
				Message = "An unexpected error occurred. Please try again later."
			}
		};
	}
}

/// <summary>
/// Standart hata response modeli.
/// </summary>
public class ErrorResponse
{
	public bool Success { get; set; }
	public ErrorDetail Error { get; set; } = null!;
}

/// <summary>
/// Hata detay modeli.
/// </summary>
public class ErrorDetail
{
	public string Code { get; set; } = null!;
	public string Message { get; set; } = null!;
	public Dictionary<string, string[]>? Details { get; set; }
}