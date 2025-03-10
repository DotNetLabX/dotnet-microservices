using System.Reflection;

namespace Blocks.Core.Extensions;

public static class AssemblyExtensions
{
		public static string GetServiceName(this Assembly assembly)
		{
				var assemblyName = assembly.GetName().Name
						?? throw new InvalidOperationException("Assembly has no name");
				return assemblyName
						.Split('.', StringSplitOptions.RemoveEmptyEntries)[0]
						.Trim()
						.ToLowerInvariant();
		}
}
