namespace Blocks.Domain;

public class DomainException(string message, Exception? innerException = null)
		: Exception(message, innerException);