namespace Domain.Entities;

public class School
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime EstablishedOn { get; set; }
}
