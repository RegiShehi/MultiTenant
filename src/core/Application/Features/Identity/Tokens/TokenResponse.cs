namespace Application.Features.Identity.Tokens;

public class TokenResponse
{
    public required string JwtToken { get; set; }

    public required string RefreshToken { get; set; }

    public DateTime RefreshTokenExpiryDate { get; set; }
}
