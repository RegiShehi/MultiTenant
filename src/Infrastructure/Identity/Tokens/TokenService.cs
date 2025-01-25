﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Features.Identity.Tokens;
using Infrastructure.Identity.Constants;
using Infrastructure.Identity.Models;
using Infrastructure.Tenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Identity.Tokens;

public class TokenService(UserManager<ApplicationUser> userManager, AbcTenantInfo tenant) : ITokenService
{
    public async Task<TokenResponse> LoginAsync(TokenRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        if (user is null) throw new UnauthorizedAccessException("Authentication failed");

        if (!await userManager.CheckPasswordAsync(user, request.Password))
            throw new UnauthorizedAccessException("Incorrect credentials");

        if (!user.IsActive) throw new UnauthorizedAccessException("User is not active. Please contact admin.");

        if (tenant.Id != TenancyConstants.Root.Id)
        {
            if (tenant.ValidUpTo < DateTime.UtcNow)
                throw new UnauthorizedAccessException("Tenant subscription has expired. Please contact admin.");
        }

        return await GenerateTokenAndUpdateUserAsync(user);
    }

    public async Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var userPrincipal = GetClaimPrincipalFromExpiredToken(request.CurrentJwtToken);

        var userEmail = userPrincipal.GetEmail();

        var user = await userManager.FindByEmailAsync(userEmail) ??
                   throw new UnauthorizedAccessException("User not found.");

        return await GenerateTokenAndUpdateUserAsync(user);
    }

    private ClaimsPrincipal GetClaimPrincipalFromExpiredToken(string expiredToken)
    {
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey("somerandomsecrettexttosetsecrettext"u8.ToArray()),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = ClaimTypes.Role,
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(expiredToken, validationParams, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256Signature,
                StringComparison.InvariantCultureIgnoreCase))
            throw new UnauthorizedAccessException("Invalid token. Failed to create refresh token.");

        return principal;
    }

    private async Task<TokenResponse> GenerateTokenAndUpdateUserAsync(ApplicationUser user)
    {
        var token = GenerateJwtToken(user);
        user.RefreshToken = GenerateRefreshToken();
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        await userManager.UpdateAsync(user);

        return new TokenResponse
        {
            JwtToken = token,
            RefreshToken = user.RefreshToken,
            RefreshTokenExpiryDate = user.RefreshTokenExpiryTime
        };
    }

    private string GenerateJwtToken(ApplicationUser user)
    {
        return GenerateEncryptedToken(GetSigningCredentials(), GetUserClaims(user));
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];

        using var randomNumberGenerator = RandomNumberGenerator.Create();
        randomNumberGenerator.GetBytes(randomNumber);

        return Convert.ToBase64String(randomNumber);
    }

    private string GenerateEncryptedToken(SigningCredentials credentials, IEnumerable<Claim> claims)
    {
        var token = new JwtSecurityToken(claims: claims, expires: DateTime.UtcNow.AddMinutes(60),
            signingCredentials: credentials);

        var tokenHandler = new JwtSecurityTokenHandler();

        return tokenHandler.WriteToken(token);
    }

    private SigningCredentials GetSigningCredentials()
    {
        var secret = "somerandomsecrettexttosetsecrettext"u8.ToArray();

        return new SigningCredentials(new SymmetricSecurityKey(secret), SecurityAlgorithms.HmacSha256Signature);
    }

    private IEnumerable<Claim> GetUserClaims(ApplicationUser user)
    {
        return new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.FirstName ?? string.Empty),
            new(ClaimTypes.Surname, user.LastName ?? string.Empty),
            new(ClaimTypes.MobilePhone, user.PhoneNumber ?? string.Empty),
            new(ClaimConstants.Tenant, tenant.Id ?? string.Empty)
        };
    }
}