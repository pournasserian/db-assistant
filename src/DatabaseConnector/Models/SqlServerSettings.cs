namespace DatabaseConnector.Models;

public class SqlServerSettings
{
    public string ConnectionString { get; set; } = "Server=localhost;Database=CS_Dispatcher;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;";
    public string Name { get; set; } = "Default";

}
