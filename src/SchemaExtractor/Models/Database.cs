namespace SchemaExtractor.Models;

public class Database
{
    public string Title { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string ConnectionString { get; set; } = default!;
    public List<Table> Tables { get; set; } = [];
    public List<View> Views { get; set; } = [];
    public List<StoredProcedure> StoreProcedures { get; set; } = [];
    public List<Function> Functions { get; set; } = [];
    //public List<TriggerInfo> Triggers { get; set; } = [];
    //public List<IndexInfo> Indexes { get; set; } = [];
    //public List<ConstraintInfo> Constraints { get; set; } = [];
    //public List<SchemaInfo> Schemas { get; set; } = [];
}
