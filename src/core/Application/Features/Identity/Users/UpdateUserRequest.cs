namespace Application.Features.Identity.Users;

public class UpdateUserRequest
{
    public required string Id { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? PhoneNumber { get; set; }
}
