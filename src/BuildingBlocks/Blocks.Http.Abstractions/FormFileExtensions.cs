using Microsoft.AspNetCore.Http;

namespace Blocks.Http.Abstractions;

public static class FormFileExtensions
{
		public static string GetExtension(this IFormFile file, string defaultExtension = "")
		{
				if (file == null)
						throw new ArgumentNullException(nameof(file));

				if (string.IsNullOrWhiteSpace(file.FileName))
						return defaultExtension;

				return Path.GetExtension(file.FileName)
									 .TrimStart('.')
									 .ToLowerInvariant();
		}
}
