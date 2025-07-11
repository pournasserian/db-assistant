namespace UI.Components.Pages;

public class DatabaseSettings
{
    public string ConnectionString { get; set; } = "Server=localhost;Database=FluentCMS;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;";
    public string Name { get; set; } = "Default";

}
