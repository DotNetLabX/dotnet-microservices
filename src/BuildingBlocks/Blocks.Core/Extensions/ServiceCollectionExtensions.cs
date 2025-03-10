using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Blocks.Core;

public static class ServiceCollectionExtensions
{
		public static IServiceCollection AddDerivedTypesOf(
				this IServiceCollection services,
				Type baseType,
				Assembly[]? assemblies = null,
				ServiceLifetime lifetime = ServiceLifetime.Scoped)
		{
				assemblies ??= [ Assembly.GetCallingAssembly() ];

				var implementations = assemblies
						.SelectMany(a => a.GetTypes())
						.Where(t =>
								t.IsClass &&
								!t.IsAbstract &&
								InheritsFrom(t, baseType))
						.ToList();

				foreach (var impl in implementations)
				{
						services.Add(new ServiceDescriptor(impl, impl, lifetime));
				}

				return services;
		}

		private static bool InheritsFrom(Type type, Type baseType)
		{
				var current = type;
				while (current != null && current != typeof(object))
				{
						// Check for exact match (non-generic inheritance)
						if (current == baseType)
								return true;

						// Check for generic type inheritance
						if (current.IsGenericType && baseType.IsGenericTypeDefinition &&
								current.GetGenericTypeDefinition() == baseType)
								return true;

						current = current.BaseType;
				}
				return false;
		}
}

