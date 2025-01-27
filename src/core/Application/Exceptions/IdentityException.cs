namespace Application.Exceptions;

using System.Net;

public class IdentityException : Exception
{
    public IdentityException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public IdentityException(string message)
        : base(message)
    {
    }

    public IdentityException()
    {
    }

    public IdentityException(
        string message,
        IReadOnlyList<string>? errorMessages,
        HttpStatusCode statusCode = HttpStatusCode.Unauthorized)
        : base(message)
    {
        ErrorMessages = errorMessages;
        StatusCode = statusCode;
    }

    public IReadOnlyList<string>? ErrorMessages { get; }

    public HttpStatusCode StatusCode { get; }
}
