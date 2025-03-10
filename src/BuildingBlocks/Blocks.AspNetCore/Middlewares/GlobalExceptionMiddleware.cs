using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FluentValidation;
using Blocks.Domain;
using Blocks.Exceptions;

namespace Blocks.AspNetCore;

public sealed class GlobalExceptionMiddleware(RequestDelegate _next, ILogger<GlobalExceptionMiddleware> _logger, IWebHostEnvironment _env)
{
		private static HttpStatusCode MapStatusCode(Exception ex) => ex switch
		{
				ValidationException => HttpStatusCode.BadRequest,
				ArgumentException => HttpStatusCode.BadRequest,
				BadRequestException => HttpStatusCode.BadRequest,
				NotFoundException => HttpStatusCode.NotFound,
				DomainException => HttpStatusCode.BadRequest,
				UnauthorizedException => HttpStatusCode.Unauthorized,
				_ => HttpStatusCode.InternalServerError
		};

		public async Task InvokeAsync(HttpContext context)
		{
				try
				{
						await _next(context);
				}
				catch (ValidationException ex)
				{
						await HandleValidationExceptionAsync(context, ex);
				}
				catch (OperationCanceledException) //response is likely abandoned by client
				{						
						if (!context.Response.HasStarted)
								context.Response.StatusCode = 499;
				}
				catch (Exception ex)
				{
						await HandleExceptionAsync(context, ex);
				}
		}

		private Task HandleExceptionAsync(HttpContext context, Exception exception)
		{
				var statusCode = MapStatusCode(exception);
				if (statusCode >= HttpStatusCode.InternalServerError) // biger than 500
				{
						_logger.LogError(exception, "Unhandled exception. TraceId={TraceId}", context.TraceIdentifier);
				}

				var response = new
				{
						StatusCode = (int)statusCode,
						exception.Message,
						TraceId = context.TraceIdentifier,
						Details = _env.IsDevelopment() ? exception.StackTrace : null
				};

				return WriteResponseAsync(context, statusCode, response);
		}

		private Task HandleValidationExceptionAsync(HttpContext context, ValidationException exception)
		{
				var validationErrors = exception.Errors.Select(e => new
				{
						e.PropertyName,
						e.ErrorMessage
				});

				var response = new
				{
						StatusCode = (int)HttpStatusCode.BadRequest,
						Message = "One or more validation errors occurred.",
						TraceId = context.TraceIdentifier,
						Errors = validationErrors,
						Details = _env.IsDevelopment() ? exception.StackTrace : null
				};

				return WriteResponseAsync(context, HttpStatusCode.BadRequest, response);
		}

		private static Task WriteResponseAsync(HttpContext context, HttpStatusCode statusCode, object response)
		{
				context.Response.StatusCode = (int)statusCode;
				context.Response.ContentType = "application/json";

				var responseJson = JsonSerializer.Serialize(response);
				return context.Response.WriteAsync(responseJson);
		}
}
