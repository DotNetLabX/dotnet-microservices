using System.Net;

namespace Blocks.Exceptions;

public class BadRequestException : HttpException
{
    public BadRequestException(string exceptionMessage) : base(HttpStatusCode.BadRequest, exceptionMessage){ }

    public BadRequestException(string exceptionMessage, Exception exception) : base(HttpStatusCode.BadRequest, exceptionMessage, exception){ }
}