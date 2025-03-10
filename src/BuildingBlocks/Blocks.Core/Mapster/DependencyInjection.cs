using Mapster;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Blocks.Mapster;
public static class DependencyInjection
{
		[MethodImpl(MethodImplOptions.NoInlining)]
		public static IServiceCollection AddMapsterConfigsFromCurrentAssembly(this IServiceCollection services, Assembly? assembly = null)
		{
				if (assembly is null)
						assembly = Assembly.GetCallingAssembly()!;
				
				TypeAdapterConfig.GlobalSettings.Scan(assembly);
				return services;
		}

		public static IServiceCollection AddMapsterConfigsFromAssemblyContaining<T>(this IServiceCollection services)
		{
				TypeAdapterConfig.GlobalSettings.Scan(typeof(T).Assembly);
				return services;
		}
}
