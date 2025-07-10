namespace DatabaseConnector.Models;

public class StoreProcedureInfo
{
    public string Name { get; set; } = default!;
    public string Schema { get; set; } = default!;
    public string? Definition { get; set; }
}