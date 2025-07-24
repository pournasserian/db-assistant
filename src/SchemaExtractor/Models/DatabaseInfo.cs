namespace SchemaExtractor.Models;

public class DatabaseInfo
{
    public string Title { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string ConnectionString { get; set; } = default!;
    public List<TableInfo> Tables { get; set; } = [];
    public List<ViewInfo> Views { get; set; } = [];
    public List<StoreProcedureInfo> StoreProcedures { get; set; } = [];
    public List<FunctionInfo> Functions { get; set; } = [];
    //public List<TriggerInfo> Triggers { get; set; } = [];
    //public List<IndexInfo> Indexes { get; set; } = [];
    //public List<ConstraintInfo> Constraints { get; set; } = [];
    //public List<SchemaInfo> Schemas { get; set; } = [];
}
