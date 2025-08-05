using ConsoleApp;
using DatabaseConnector;
using DatabaseConnector.Models;

var sqlServerSettings = new SqlServerSettings
{
    ConnectionString = "Server=localhost;Database=CS_Dispatcher;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;",
    Name = "CS_Dispatcher"
};

DatabaseInfo databaseInfo = default!;

var databaseConnector = new SqlServerConnector(sqlServerSettings);

Console.WriteLine("Connecting to database...");

if (await databaseConnector.TryConnect())
{
    Console.WriteLine("Connected successfully...");
    databaseInfo = await databaseConnector.GetDatabase(sqlServerSettings.Name);
}
else
{
    Console.WriteLine("Failed to connect to database.");
    return;
}

var repository = new Repository();
Console.WriteLine("Saving database schema info to repository...");
await repository.SaveServer(databaseInfo);
Console.WriteLine("Database schema info saved successfully.");

