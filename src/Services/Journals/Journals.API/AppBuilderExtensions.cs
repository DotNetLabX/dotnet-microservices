using Journals.Domain.Journals;
using Redis.OM;

namespace Journals.API;

public static class AppBuilderExtensions
{
		public static IApplicationBuilder UseRedis(this IApplicationBuilder app)
		{
				using (var scope = app.ApplicationServices.CreateScope())
				{
						var provider = scope.ServiceProvider.GetRequiredService<RedisConnectionProvider>();
						provider.Connection.CreateIndex(typeof(Editor));
						provider.Connection.CreateIndex(typeof(Journal));
				}

				return app;
		}
}
