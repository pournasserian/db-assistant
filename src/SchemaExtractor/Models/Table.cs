namespace SchemaExtractor.Models;

public class Table
{
    public string Name { get; set; } = default!;
    public string Schema { get; set; } = default!;
    public List<Column> Columns { get; set; } = [];
}