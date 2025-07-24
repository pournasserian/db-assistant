using Microsoft.Data.SqlClient;
using SchemaExtractor.Models;
using System.Data.Common;

namespace SchemaExtractor;

public class SqlServerSchemaExtractor : ISchemaExtractor
{
    public readonly SqlServerSettings Settings = default!;
    private readonly string _connectionString = default!;

    public SqlServerSchemaExtractor(SqlServerSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));
        ArgumentException.ThrowIfNullOrEmpty(settings.ConnectionString, nameof(settings.ConnectionString));

        Settings = settings;
        _connectionString = settings.ConnectionString;

        try
        {
            using var _connection = new SqlConnection(_connectionString);
            _connection.Open();

            // Test the connection by executing a simple query
            using var command = new SqlCommand("SELECT 1", _connection);
            command.ExecuteScalar();

            _connection.Close();

        }
        catch (SqlException)
        {
            throw new InvalidOperationException("Failed to connect to the SQL Server database. Please check your connection string and database availability.");
        }
        catch (DbException ex)
        {
            throw new InvalidOperationException("An error occurred while connecting to the database.", ex);
        }
    }

    // Get all tables in the database with their schemas
    public async Task<List<Table>> ExtractTables(CancellationToken cancellationToken = default)
    {
        var tables = new List<Table>();

        string query = @"
            SELECT 
                t.name AS TableName,
                s.name AS SchemaName,
                t.create_date AS CreatedDate,
                t.modify_date AS ModifiedDate
            FROM 
                sys.tables t
            INNER JOIN 
                sys.schemas s ON t.schema_id = s.schema_id
            INNER JOIN 
                sys.indexes i ON t.object_id = i.object_id
            INNER JOIN 
                sys.partitions p ON i.object_id = p.object_id AND i.index_id = p.index_id
            WHERE 
                i.index_id < 2
            ORDER BY 
                TableName";

        using (var _connection = new SqlConnection(_connectionString))
        {
            using var command = new SqlCommand(query, _connection);
            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                tables.Add(new Table
                {
                    Schema = reader["SchemaName"].ToString()!,
                    Name = reader["TableName"].ToString()!
                });
            }

            foreach (var table in tables)
                table.Columns = await ExtractColumns(_connection, table.Schema, table.Name, cancellationToken);
        }
        return tables;
    }

    private static async Task<List<Column>> ExtractColumns(SqlConnection sqlConnection, string schemaName, string tableName, CancellationToken cancellationToken = default)
    {
        var columns = new List<Column>();

        string query = @"
            SELECT 
                c.name AS ColumnName,
                t.name AS DataType,
                c.max_length AS MaxLength,
                c.precision AS Precision,
                c.scale AS Scale,
                c.is_nullable AS IsNullable,
                c.is_identity AS IsIdentity,
                OBJECT_NAME(dc.object_id) AS DefaultConstraint,
                dc.definition AS DefaultValue
            FROM 
                sys.columns c
            INNER JOIN 
                sys.types t ON c.user_type_id = t.user_type_id
            LEFT JOIN 
                sys.default_constraints dc ON c.default_object_id = dc.object_id
            WHERE 
                c.object_id = OBJECT_ID(@TableName)
            ORDER BY 
                c.column_id";

        using (var command = new SqlCommand(query, sqlConnection))
        {
            command.Parameters.AddWithValue("@TableName", $"{schemaName}.{tableName}");

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                columns.Add(new Column
                {
                    Name = reader["ColumnName"].ToString()!,
                    DataType = reader["DataType"].ToString()!,
                    MaxLength = int.Parse(reader["MaxLength"].ToString()!),
                    Precision = int.Parse(reader["Precision"].ToString()!),
                    Scale = int.Parse(reader["Scale"].ToString()!),
                    IsNullable = bool.Parse(reader["IsNullable"].ToString()!),
                    IsIdentity = bool.Parse(reader["IsIdentity"].ToString()!),
                    DefaultConstraint = reader["DefaultConstraint"] == DBNull.Value ? null : reader["DefaultConstraint"].ToString(),
                    DefaultValue = reader["DefaultValue"] == DBNull.Value ? null : reader["DefaultValue"].ToString()
                });
            }
        }

        return columns;
    }

    public async Task<List<View>> ExtractViews(CancellationToken cancellationToken = default)
    {
        var views = new List<View>();

        string query = @"
            SELECT 
                v.name AS ViewName,
                s.name AS SchemaName,
                v.create_date AS CreatedDate,
                v.modify_date AS ModifiedDate,
                OBJECT_DEFINITION(v.object_id) AS Definition
            FROM 
                sys.views v
            INNER JOIN 
                sys.schemas s ON v.schema_id = s.schema_id
            ORDER BY 
                ViewName";

        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand(query, connection))
        {
            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                views.Add(new View
                {
                    Name = reader["ViewName"].ToString()!,
                    Schema = reader["SchemaName"].ToString()!,
                    Definition = reader["Definition"].ToString()
                });
            }
        }

        return views;
    }

    public async Task<List<StoredProcedure>> ExtractStoredProcedures(CancellationToken cancellationToken = default)
    {
        var procedures = new List<StoredProcedure>();

        string query = @"
            SELECT 
                p.name AS ProcedureName,
                s.name AS SchemaName,
                p.create_date AS CreatedDate,
                p.modify_date AS ModifiedDate,
                OBJECT_DEFINITION(p.object_id) AS Definition
            FROM 
                sys.procedures p
            INNER JOIN 
                sys.schemas s ON p.schema_id = s.schema_id
            ORDER BY 
                ProcedureName";

        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand(query, connection))
        {
            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                procedures.Add(new StoredProcedure
                {
                    Name = reader["ProcedureName"].ToString()!,
                    Schema = reader["SchemaName"].ToString()!,
                    Definition = reader["Definition"].ToString()
                });
            }
        }

        return procedures;
    }

    public async Task<List<Function>> ExtractFunctions(CancellationToken cancellationToken = default)
    {
        var functions = new List<Function>();

        string query = @"
            SELECT 
                f.name AS FunctionName,
                s.name AS SchemaName,
                f.create_date AS CreatedDate,
                f.modify_date AS ModifiedDate,
                OBJECT_DEFINITION(f.object_id) AS Definition,
                CASE f.type
                    WHEN 'IF' THEN 'Inline Table-valued Function'
                    WHEN 'TF' THEN 'Table-valued Function'
                    WHEN 'FN' THEN 'Scalar Function'
                    ELSE 'Unknown'
                END AS FunctionType
            FROM 
                sys.objects f
            INNER JOIN 
                sys.schemas s ON f.schema_id = s.schema_id
            WHERE 
                f.type IN ('IF', 'TF', 'FN')
            ORDER BY 
                FunctionName";

        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand(query, connection))
        {
            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                functions.Add(new Function
                {
                    Name = reader["FunctionName"].ToString()!,
                    Schema = reader["SchemaName"].ToString()!,
                    Definition = reader["Definition"].ToString(),
                    Type = reader["FunctionType"].ToString()!
                });
            }
        }

        return functions;
    }
}