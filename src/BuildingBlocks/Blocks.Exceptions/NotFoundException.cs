using System.Net;

namespace Blocks.Exceptions;

public class NotFoundException : HttpException
{
    public NotFoundException(string exceptionMessage) : base(HttpStatusCode.NotFound, exceptionMessage){ }

    public NotFoundException(Exception exception, string? exceptionMessage) : base(HttpStatusCode.NotFound, exceptionMessage, exception){ }
}