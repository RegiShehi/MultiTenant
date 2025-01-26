namespace Application.Features.Identity.Tokens;

public class RefreshTokenRequest
{
    public required string CurrentJwtToken { get; set; }

    public required string CurrentRefreshToken { get; set; }
}
