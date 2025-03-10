using System.Security.Cryptography;

namespace Blocks.Core.Security;

public static class Base64UrlTokenGenerator
{
		public static string Generate(int byteLength = 32)
		{
				byte[] randomBytes = RandomNumberGenerator.GetBytes(byteLength);
				return ToBase64Url(randomBytes);
		}

		private static string ToBase64Url(byte[] bytes)
		{
				string base64 = Convert.ToBase64String(bytes);
				return base64
						.Replace("+", "-")
						.Replace("/", "_")
						.TrimEnd('='); // Remove padding
		}
}
