using SchemaExtractor.Models;

namespace SchemaExtractor;

public interface ISchemaExtractor
{
    Task<List<Table>> ExtractTables(CancellationToken cancellationToken = default);
    Task<List<View>> ExtractViews(CancellationToken cancellationToken = default);
    Task<List<StoredProcedure>> ExtractStoredProcedures(CancellationToken cancellationToken = default);
    Task<List<Function>> ExtractFunctions(CancellationToken cancellationToken = default);
}