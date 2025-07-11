using DatabaseConnector.Models;
using Microsoft.Data.SqlClient;

namespace DatabaseConnector
{
    public interface IDatabaseConnector
    {
        SqlConnection Connection { get; set; }
        string ConnectionString { get; set; }

        Task Disconnect(CancellationToken cancellationToken = default);
        Task<DatabaseInfo> GetDatabase(string name, CancellationToken cancellationToken = default);
        Task<bool> TryConnect(CancellationToken cancellationToken = default);
    }
}