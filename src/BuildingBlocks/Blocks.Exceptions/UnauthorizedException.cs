using System.Net;

namespace Blocks.Exceptions;

public class UnauthorizedException : HttpException
{
		public UnauthorizedException(string? message) : base(HttpStatusCode.Unauthorized, message) { }
}
