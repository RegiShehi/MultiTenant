namespace Infrastructure.Persistence.DbConfigurations;

using Domain.Entities;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SchoolConfig : IEntityTypeConfiguration<School>
{
    public void Configure(EntityTypeBuilder<School> builder)
    {
        builder
            .ToTable("School", SchemaNames.Academics)
            .IsMultiTenant();

        builder
            .Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);
    }
}
