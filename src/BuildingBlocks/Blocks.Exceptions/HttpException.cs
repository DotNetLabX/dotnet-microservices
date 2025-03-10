using System.Net;

namespace Blocks.Exceptions;

public class HttpException : Exception
{
    public HttpException(HttpStatusCode statusCode, string? message, Exception ex)
        : base(message, ex)
    {
        this.HttpStatusCode = statusCode;
    }

    public HttpException(HttpStatusCode statusCode, string? message)
        : base(string.IsNullOrEmpty(message) ? statusCode.ToString() : message)
    {
        this.HttpStatusCode = statusCode;
    }

    public HttpStatusCode HttpStatusCode { get; }

    public int StatusCode => (int) HttpStatusCode;
}
