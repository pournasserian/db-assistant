namespace DatabaseConnector.Models;

public class ColumnInfo
{
    public string Name { get; set; } = default!;
    public string DataType { get; set; } = default!;
    public int Precision { get; set; }
    public int Scale { get; set; }
    public bool IsIdentity { get; set; }
    public string? DefaultConstraint { get; set; } = default!;
    public string? DefaultValue { get; set; } = default!;
    public bool IsNullable { get; set; }
    public int MaxLength { get; set; }
    public bool IsPrimaryKey { get; set; }
}
