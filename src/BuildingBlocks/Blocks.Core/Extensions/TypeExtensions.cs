using System.Runtime.CompilerServices;
namespace Blocks.Core.Extensions;

public static class TypeExtensions
{
		public static bool IsRecord(this Type type)
		{
				// Check if the type is a class and has the CompilerGenerated attribute
				return type.IsClass &&
							 type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), true).Any();
		}
}
