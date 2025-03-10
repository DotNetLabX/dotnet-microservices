using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Reflection;
using System.Text.Json;

namespace Blocks.AspNetCore.ModelBinding;

public class GenericModelBinder<T> : IModelBinder where T : class, new()
{
		public Task BindModelAsync(ModelBindingContext bindingContext)
		{
				var httpContext = bindingContext.HttpContext;
				var routeValues = httpContext.Request.RouteValues;

				var model = new T();
				var modelType = typeof(T);

				foreach (var kvp in routeValues)
				{
						var prop = modelType.GetProperty(kvp.Key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
						if (prop != null && prop.CanWrite)
						{
								var value = Convert.ChangeType(kvp.Value, prop.PropertyType);
								prop.SetValue(model, value);
						}
				}

				// Optionally bind body too if needed
				if (httpContext.Request.ContentLength > 0 &&
						httpContext.Request.ContentType?.Contains("application/json") == true)
				{
						httpContext.Request.Body.Position = 0;
						var bodyModel = JsonSerializer.Deserialize<T>(httpContext.Request.Body, new JsonSerializerOptions
						{
								PropertyNameCaseInsensitive = true
						});

						// Merge body into existing model (overriding route values if needed)
						foreach (var prop in modelType.GetProperties().Where(p => p.CanWrite))
						{
								var bodyValue = prop.GetValue(bodyModel);
								if (bodyValue != null)
										prop.SetValue(model, bodyValue);
						}
				}

				bindingContext.Result = ModelBindingResult.Success(model);
				return Task.CompletedTask;
		}
}
