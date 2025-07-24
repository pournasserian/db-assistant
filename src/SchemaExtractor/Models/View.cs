namespace SchemaExtractor.Models;

public class View
{
    public string Name { get; set; } = default!;
    public string Schema { get; set; } = default!;
    public string? Definition { get; set; }
}
