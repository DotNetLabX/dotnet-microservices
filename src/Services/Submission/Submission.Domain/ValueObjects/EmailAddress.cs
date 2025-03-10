using Blocks.Core;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Submission.Domain.ValueObjects;

public class EmailAddress : StringValueObject
{
		[JsonConstructor]
		private EmailAddress(string value) => Value = value;

		public static EmailAddress Create(string value)
		{
				Guard.ThrowIfNullOrWhiteSpace(value);
				Guard.ThrowIfFalse(IsValidEmail(value), "Invalid email format.");

				return new EmailAddress(value.ToLower());
		}

		private static bool IsValidEmail(string email)
		{
				// Basic email regex for validation
				const string emailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
				return Regex.IsMatch(email, emailRegex, RegexOptions.IgnoreCase);
		}
}
