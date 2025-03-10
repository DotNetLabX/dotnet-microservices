using Blocks.Entitities;
using System.Collections.Immutable;

namespace Production.Domain.Assets.ValueObjects;

public class AllowedFileExtensions : IValueObject
{
		public IReadOnlyList<string> Extensions { get; init; } = null!;

		public bool IsValidExtension(string extension) 
				=> Extensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
}
