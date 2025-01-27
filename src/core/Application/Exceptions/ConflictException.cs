namespace Application.Exceptions;

using System.Net;

public class ConflictException : Exception
{
    public ConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public ConflictException(string message)
        : base(message)
    {
    }

    public ConflictException()
    {
    }

    public ConflictException(
        string message,
        IReadOnlyList<string>? errorMessages,
        HttpStatusCode statusCode = HttpStatusCode.Conflict)
        : base(message)
    {
        ErrorMessages = errorMessages;
        StatusCode = statusCode;
    }

    public IReadOnlyList<string>? ErrorMessages { get; }

    public HttpStatusCode StatusCode { get; }
}
