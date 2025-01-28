namespace Application.Features.Identity.Users;

public class CreateUserRequest
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public required string Email { get; set; }

    public bool IsActive { get; set; }

    public required string UserName { get; set; }

    public required string Password { get; set; }

    public required string ConfirmPassword { get; set; }

    public string? PhoneNumber { get; set; }
}
