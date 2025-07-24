namespace SchemaExtractor.Models;

public class TableInfo
{
    public string Name { get; set; } = default!;
    public string Schema { get; set; } = default!;
    public List<ColumnInfo> Columns { get; set; } = [];
}