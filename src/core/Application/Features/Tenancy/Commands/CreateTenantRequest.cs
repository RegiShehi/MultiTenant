namespace Application.Features.Tenancy.Commands;

public class CreateTenantRequest
{
    public required string Identifier { get; set; }

    public required string Name { get; set; }

    public required string ConnectionString { get; set; }

    public required string AdminEmail { get; set; }

    public DateTime ValidUpTo { get; set; }

    public bool IsActive { get; set; }
}
