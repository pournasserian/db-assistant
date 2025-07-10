using DatabaseConnector.Models;
using Microsoft.Data.SqlClient;
using System.Data.Common;

namespace DatabaseConnector;

public class SqlServer(string connectionString)
{
    public string ConnectionString { get; set; } = connectionString;

    public SqlConnection Connection { get; set; } = default!;

    public async Task<bool> TryConnect(CancellationToken cancellationToken = default)
    {
        if (Connection is not null)
        {
            return true;
        }
        try
        {
            Connection = new SqlConnection(ConnectionString);
            await Connection.OpenAsync(cancellationToken);

            // Test the connection by executing a simple query
            using var command = new SqlCommand("SELECT 1", Connection);
            await command.ExecuteScalarAsync(cancellationToken);

            return true;
        }
        catch (SqlException)
        {
            Connection = null!;
            return false;
        }
    }

    public async Task Disconnect(CancellationToken cancellationToken = default)
    {
        if (Connection is not null)
        {
            await Connection.CloseAsync();
            await Connection.DisposeAsync();
            Connection = null!;
        }
    }

    // Get all tables in the database with their schemas
    public async Task<List<TableInfo>> GetTables(CancellationToken cancellationToken = default)
    {
        EnsureConnection();

        var tables = new List<TableInfo>();

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

        using (var command = new SqlCommand(query, Connection))
        {
            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                tables.Add(new TableInfo
                {
                    Schema = reader["SchemaName"].ToString()!,
                    Name = reader["TableName"].ToString()!
                });
            }
        }

        foreach (var table in tables)
            table.Columns = await GetColumns(table.Schema, table.Name, cancellationToken);

        return tables;
    }

    private async Task<List<ColumnInfo>> GetColumns(string schemaName, string tableName, CancellationToken cancellationToken = default)
    {
        EnsureConnection();

        var columns = new List<ColumnInfo>();

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

        using (var command = new SqlCommand(query, Connection))
        {
            command.Parameters.AddWithValue("@TableName", $"{schemaName}.{tableName}");

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                columns.Add(new ColumnInfo
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

    public async Task<List<ViewInfo>> GetViews(CancellationToken cancellationToken = default)
    {
        EnsureConnection();

        var views = new List<ViewInfo>();

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

        using (var command = new SqlCommand(query, Connection))
        {
            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                views.Add(new ViewInfo
                {
                    Name = reader["ViewName"].ToString()!,
                    Schema = reader["SchemaName"].ToString()!,
                    Definition = reader["Definition"].ToString()
                });
            }
        }

        return views;
    }

    public async Task<List<StoreProcedureInfo>> GetStoredProcedures(CancellationToken cancellationToken = default)
    {
        EnsureConnection();

        var procedures = new List<StoreProcedureInfo>();

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

        using (var command = new SqlCommand(query, Connection))
        {
            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                procedures.Add(new StoreProcedureInfo
                {
                    Name = reader["ProcedureName"].ToString()!,
                    Schema = reader["SchemaName"].ToString()!,
                    Definition = reader["Definition"].ToString()
                });
            }
        }

        return procedures;
    }

    public async Task<List<FunctionInfo>> GetFunctions(CancellationToken cancellationToken = default)
    {
        EnsureConnection();

        var functions = new List<FunctionInfo>();

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

        using (var command = new SqlCommand(query, Connection))
        {
            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                functions.Add(new FunctionInfo
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

    private void EnsureConnection()
    {
        if (Connection is null)
        {
            throw new InvalidOperationException("Database connection is not established.");
        }
    }
}