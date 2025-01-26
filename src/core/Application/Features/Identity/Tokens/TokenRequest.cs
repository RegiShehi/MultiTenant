namespace Application.Features.Identity.Tokens;

public class TokenRequest
{
    public required string Email { get; set; }

    public required string Password { get; set; }
}
