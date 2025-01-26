namespace Application.Exceptions;

using System.Net;

public class UnauthorizedException : Exception
{
    // Parameterless constructor
    public UnauthorizedException()
        : this("Unauthorized access.", null, HttpStatusCode.Unauthorized)
    {
    }

    // Constructor with message
    public UnauthorizedException(string message)
        : this(message, null, HttpStatusCode.Unauthorized)
    {
    }

    // Constructor with message and inner exception
    public UnauthorizedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    // Primary constructor without default parameters
    public UnauthorizedException(string message, IReadOnlyList<string>? errorMessages, HttpStatusCode statusCode)
        : base(message)
    {
        ErrorMessages = errorMessages;
        StatusCode = statusCode;
    }

    public IReadOnlyList<string>? ErrorMessages { get; }

    public HttpStatusCode StatusCode { get; }
}
