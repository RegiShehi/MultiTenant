namespace Application.Exceptions;

using System.Net;

public class UnauthorizedException : Exception
{
    public UnauthorizedException(
        string message,
        IReadOnlyList<string>? errorMessages = null,
        HttpStatusCode statusCode = HttpStatusCode.Unauthorized)
        : base(message)
    {
        ErrorMessages = errorMessages;
        StatusCode = statusCode;
    }

    public IReadOnlyList<string>? ErrorMessages { get; }
    public HttpStatusCode StatusCode { get; }


    public UnauthorizedException()
        : this("Unauthorized access.", null, HttpStatusCode.Unauthorized)
    {
    }

    public UnauthorizedException(string message) : this(message, null, HttpStatusCode.Unauthorized)
    {
    }

    public UnauthorizedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
