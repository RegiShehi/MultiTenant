﻿namespace Application.Exceptions;

using System.Net;

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public UnauthorizedException(string message)
        : base(message)
    {
    }

    public UnauthorizedException()
    {
    }

    public UnauthorizedException(
        string message,
        IReadOnlyList<string>? errorMessages,
        HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
        : base(message)
    {
        ErrorMessages = errorMessages;
        StatusCode = statusCode;
    }

    public IReadOnlyList<string>? ErrorMessages { get; }

    public HttpStatusCode StatusCode { get; }
}
