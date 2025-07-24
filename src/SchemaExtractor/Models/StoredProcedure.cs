namespace SchemaExtractor.Models;

public class StoredProcedure
{
    public string Name { get; set; } = default!;
    public string Schema { get; set; } = default!;
    public string? Definition { get; set; }
}