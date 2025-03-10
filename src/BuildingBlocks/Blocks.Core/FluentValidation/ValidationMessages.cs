namespace Blocks.FluentValidation;

public static class ValidationMessages
{
		public static readonly string InvalidId = "The {0} should be greater than zero.";
		public static readonly string MaxLengthExceeded = "{0} must not exceed {1} characters.";
		public static readonly string NullOrEmptyValue = "{0} is required.";
}
