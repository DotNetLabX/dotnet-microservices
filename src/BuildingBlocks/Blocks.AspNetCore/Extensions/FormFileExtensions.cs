using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;

namespace Blocks.AspNetCore;

public static class FormFileExtensions
{
		public const string DefaultFileContentType = "application/octet-stream";
		private static readonly FileExtensionContentTypeProvider _provider = new();

		public static string GetContentType(this IFormFile file, bool preferHeader = true, string fallback = DefaultFileContentType)
		{
				if (file is null) throw new ArgumentNullException(nameof(file));

				if (preferHeader && !string.IsNullOrWhiteSpace(file.ContentType) && file.ContentType != DefaultFileContentType)
						return file.ContentType;

				if (!string.IsNullOrWhiteSpace(file.FileName) &&
						_provider.TryGetContentType(file.FileName, out var fromExt) &&
						!string.IsNullOrWhiteSpace(fromExt))
						return fromExt;

				return fallback;
		}
}
