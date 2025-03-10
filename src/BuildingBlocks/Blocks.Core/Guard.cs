using Blocks.Exceptions;

namespace Blocks.Core;

public static class Guard
{
		public static void ThrowIfNullOrWhiteSpace(string value)
				=> ArgumentException.ThrowIfNullOrWhiteSpace(value);

		public static void ThrowIfNotEqual<T>(T value, T other) where T : IEquatable<T>?
				=> ArgumentOutOfRangeException.ThrowIfNotEqual(value, other);

		public static void ThrowIfNull<T>(T? value, string name)
		{
				if (value is null)
						throw new ArgumentNullException(name);
		}

		public static void ThrowIfFalse(this bool condition, string message = "Condition must be true.")
		{
				if (!condition)
						throw new ArgumentException(message);
		}

		public static T AgainstNull<T>(T? value, string parameterName)
				=> value ?? throw new ArgumentNullException(parameterName, $"Value cannot be null: '{parameterName}'.");

		public static T NotFound<T>(T? value) where T : class
				=> value ?? throw new NotFoundException($"{typeof(T).Name} not found");
}
