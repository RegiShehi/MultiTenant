﻿namespace Application.Features.Identity.Users;

public class UpdateUserRequest
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? PhoneNumber { get; set; }
}
