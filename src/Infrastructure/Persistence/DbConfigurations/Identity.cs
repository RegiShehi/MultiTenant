using Finbuckle.MultiTenant.EntityFrameworkCore;
using Infrastructure.Identity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.DbConfigurations;

internal class ApplicationUserConfig : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder
            .ToTable("Users", SchemaNames.Identity)
            .IsMultiTenant().AdjustUniqueIndexes();
    }
}

internal class ApplicationUserClaimConfig : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder
            .ToTable("RoleClaims", SchemaNames.Identity)
            .IsMultiTenant();
    }
}

internal class IdentityUserRoleConfig : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder
            .ToTable("UserRoles", SchemaNames.Identity)
            .IsMultiTenant();
    }
}

internal class IdentityUserClaimConfig : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder
            .ToTable("UserClaims", SchemaNames.Identity)
            .IsMultiTenant();
    }
}

internal class IdentityUserTokenConfig : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder
            .ToTable("UserTokens", SchemaNames.Identity)
            .IsMultiTenant();
    }
}