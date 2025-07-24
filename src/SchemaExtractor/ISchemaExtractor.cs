using SchemaExtractor.Models;

namespace SchemaExtractor;

public interface ISchemaExtractor
{
    Task<List<TableInfo>> ExtractTables(CancellationToken cancellationToken = default);
    Task<List<ViewInfo>> ExtractViews(CancellationToken cancellationToken = default);
    Task<List<StoreProcedureInfo>> ExtractStoredProcedures(CancellationToken cancellationToken = default);
    Task<List<FunctionInfo>> ExtractFunctions(CancellationToken cancellationToken = default);
}