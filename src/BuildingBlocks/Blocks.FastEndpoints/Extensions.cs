using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using System.Text.Json.Serialization;

namespace Blocks.FastEndpoints;

public static class Extensions
{
		public static IApplicationBuilder UseCustomFastEndpoints(this IApplicationBuilder app)
		{
				app.UseFastEndpoints(c =>
				{
						c.Endpoints.RoutePrefix = "api";
						c.Serializer.Options.Converters.Add(new JsonStringEnumConverter());
						c.Errors.ResponseBuilder = (failures, ctx, statusCode) =>
						{
								var errorDict = failures
										.GroupBy(f => f.PropertyName)
										.ToDictionary(
												g => g.Key,
												g => g.Select(f => f.ErrorMessage).ToArray()
										);

								var message = failures.Count == 1
										? failures.First().ErrorMessage
										: "Validation failed for multiple fields.";

								return new
								{
										statusCode,
										message,
										errors = errorDict
								};
						};
				});

				return app;
		}
}
