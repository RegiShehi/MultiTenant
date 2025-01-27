namespace Application.Exceptions;

using System.Net;

public class NotFoundException : Exception
{
    public NotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public NotFoundException(string message)
        : base(message)
    {
    }

    public NotFoundException()
    {
    }

    public NotFoundException(
        string message,
        IReadOnlyList<string>? errorMessages,
        HttpStatusCode statusCode = HttpStatusCode.NotFound)
        : base(message)
    {
        ErrorMessages = errorMessages;
        StatusCode = statusCode;
    }

    public IReadOnlyList<string>? ErrorMessages { get; }

    public HttpStatusCode StatusCode { get; }
}
