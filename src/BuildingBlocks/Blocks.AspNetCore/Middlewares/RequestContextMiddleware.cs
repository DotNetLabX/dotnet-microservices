using Blocks.Core.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Blocks.AspNetCore.Middlewares;

public sealed class RequestContextMiddleware(RequestDelegate _next, ILogger<RequestContextMiddleware> _log)
{
		private const string CorrelationIDHeader = "X-Correlation-ID";

		public async Task InvokeAsync(HttpContext httpContext, RequestContext requestContext)
		{
				var correlationId = ResolveCorrelationId(httpContext);	

				httpContext.Items[CorrelationIDHeader] = correlationId;
				requestContext.CorrelationId = correlationId;

				requestContext.IsUpload = (httpContext.Request.ContentType?.StartsWith("multipart/form-data", StringComparison.OrdinalIgnoreCase) ?? false);
				requestContext.IsDownload = httpContext.Request.Path.Value?.IndexOf("download", StringComparison.OrdinalIgnoreCase) >= 0;
				requestContext.RemoteIp = httpContext.Connection.RemoteIpAddress?.ToString(); // client IP

				// send back the CorrelationId in the response
				httpContext.Response.OnStarting(() =>
				{
						if (!httpContext.Response.Headers.ContainsKey(CorrelationIDHeader))
								httpContext.Response.Headers[CorrelationIDHeader] = requestContext.CorrelationId ?? string.Empty;
						return Task.CompletedTask;
				});

				// opens a logging scope for the rest of the request,
				// so every log line written inside will be enriched with the X-Correlation-ID.
				using (_log.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = requestContext.CorrelationId ?? "" }))
				{
						await _next(httpContext);
				}
		}

		private static string ResolveCorrelationId(HttpContext httpContext)
		{
				if (httpContext.Request.Headers.TryGetValue(CorrelationIDHeader, out var value) && !string.IsNullOrWhiteSpace(value))
						return value.ToString();

				// fallback to distributed tracing ID (activityId) if this request has a trace(id) parent
				var activityId = Activity.Current?.TraceId.ToString();
				if (!string.IsNullOrEmpty(activityId))
						return activityId!;

				// fallback to TraceIdentifier (kestrel request id) or just generate a new ID
				return httpContext.TraceIdentifier; // or Guid.NewGuid().ToString();
		}
}
