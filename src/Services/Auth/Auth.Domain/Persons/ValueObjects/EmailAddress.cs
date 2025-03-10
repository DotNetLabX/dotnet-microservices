using Blocks.Core;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Auth.Domain.Persons.ValueObjects;

public class EmailAddress : StringValueObject
{
		[JsonConstructor]
		internal EmailAddress(string value)
		{
				Value = value;
				NormalizedEmail = value.ToUpperInvariant();
		}
		
		public string NormalizedEmail { get; internal set; }

		public static EmailAddress Create(string value)
		{
				Guard.ThrowIfNullOrWhiteSpace(value);
				Guard.ThrowIfFalse(IsValidEmail(value), "Invalid email format.");

				return new EmailAddress(value);
		}

		private static bool IsValidEmail(string email)
		{
				// Basic email regex for validation
				const string emailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
				return Regex.IsMatch(email, emailRegex, RegexOptions.IgnoreCase);
		}

		public static implicit operator EmailAddress(string value)
				=> Create(value);

		public static implicit operator string(EmailAddress email)
				=> email.Value;

		public override int GetHashCode()
				=> NormalizedEmail.GetHashCode();
}
