namespace Infrastructure.Identity.Tokens;

using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Features.Identity.Tokens;
using Authentication.Jwt;
using Constants;
using Models;
using Tenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

public class TokenService(
    UserManager<ApplicationUser> userManager,
    AbcTenantInfo tenant,
    IOptions<JwtSettings> jwtSettings) : ITokenService
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;

    public async Task<TokenResponse> LoginAsync(TokenRequest request)
    {
        ApplicationUser user = await userManager.FindByEmailAsync(request.Email) ??
                               throw new UnauthorizedAccessException("Authentication failed");

        if (!await userManager.CheckPasswordAsync(user, request.Password))
        {
            throw new UnauthorizedAccessException("Incorrect credentials");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("User is not active. Please contact admin.");
        }

        bool validSubscription = tenant.Id != TenancyConstants.Root.Id && tenant.ValidUpTo < DateTime.UtcNow;

        return validSubscription
            ? await GenerateTokenAndUpdateUserAsync(user)
            : throw new UnauthorizedAccessException("Tenant subscription has expired. Please contact admin.");
    }

    public async Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        ClaimsPrincipal userPrincipal = GetClaimPrincipalFromExpiredToken(request.CurrentJwtToken);

        string userEmail = userPrincipal.GetEmail();

        ApplicationUser user = await userManager.FindByEmailAsync(userEmail) ??
                               throw new UnauthorizedAccessException("User not found.");

        return await GenerateTokenAndUpdateUserAsync(user);
    }

    private static string GenerateRefreshToken()
    {
        byte[] randomNumber = new byte[32];

        using var randomNumberGenerator = RandomNumberGenerator.Create();
        randomNumberGenerator.GetBytes(randomNumber);

        return Convert.ToBase64String(randomNumber);
    }

    private SigningCredentials GetSigningCredentials()
    {
        byte[] secret = Encoding.UTF8.GetBytes(_jwtSettings.Key);

        return new SigningCredentials(new SymmetricSecurityKey(secret), SecurityAlgorithms.HmacSha256Signature);
    }

    private string GenerateEncryptedToken(SigningCredentials credentials, IEnumerable<Claim> claims)
    {
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.TokenExpiryTimeInMinutes),
            signingCredentials: credentials);

        var tokenHandler = new JwtSecurityTokenHandler();

        return tokenHandler.WriteToken(token);
    }

    [SuppressMessage(
        "Security",
        "CA5404:Do not disable token validation checks",
        Justification = "Just for testing purpose")]
    private ClaimsPrincipal GetClaimPrincipalFromExpiredToken(string expiredToken)
    {
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = ClaimTypes.Role,
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        ClaimsPrincipal? principal =
            tokenHandler.ValidateToken(expiredToken, validationParams, out SecurityToken? securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(
                SecurityAlgorithms.HmacSha256Signature,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Invalid token. Failed to create refresh token.");
        }

        return principal;
    }

    private async Task<TokenResponse> GenerateTokenAndUpdateUserAsync(ApplicationUser user)
    {
        string token = GenerateJwtToken(user);
        user.RefreshToken = GenerateRefreshToken();
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryTimeInDays);

        await userManager.UpdateAsync(user);

        return new TokenResponse
        {
            JwtToken = token,
            RefreshToken = user.RefreshToken,
            RefreshTokenExpiryDate = user.RefreshTokenExpiryTime
        };
    }

    private string GenerateJwtToken(ApplicationUser user) =>
        GenerateEncryptedToken(GetSigningCredentials(), GetUserClaims(user));

    private List<Claim> GetUserClaims(ApplicationUser user) =>
    [
        new(ClaimTypes.NameIdentifier, user.Id),
        new(ClaimTypes.Email, user.Email ?? string.Empty),
        new(ClaimTypes.Name, user.FirstName ?? string.Empty),
        new(ClaimTypes.Surname, user.LastName ?? string.Empty),
        new(ClaimTypes.MobilePhone, user.PhoneNumber ?? string.Empty),
        new(ClaimConstants.Tenant, tenant.Id ?? string.Empty)
    ];
}
