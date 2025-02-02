namespace Application.Exceptions;

using System.Net;

public class ForbiddenException : Exception
{
    public ForbiddenException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public ForbiddenException(string message)
        : base(message)
    {
    }

    public ForbiddenException()
    {
    }

    public ForbiddenException(
        string message,
        IReadOnlyList<string>? errorMessages,
        HttpStatusCode statusCode = HttpStatusCode.Forbidden)
        : base(message)
    {
        ErrorMessages = errorMessages;
        StatusCode = statusCode;
    }

    public IReadOnlyList<string>? ErrorMessages { get; }

    public HttpStatusCode StatusCode { get; }
}
