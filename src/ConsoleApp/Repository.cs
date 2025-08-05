using DatabaseConnector.Models;

namespace ConsoleApp;

public class Repository
{
    public const string REPOSITORY_FOLDER = "Repository";
    public const string MANIFEST_FILE = "manifest.json";

    public Task<List<string>> GetServers()
    {
        // Get all folders in the repository folder
        var repositoryPath = REPOSITORY_FOLDER;
        if (!Directory.Exists(repositoryPath))
        {
            Directory.CreateDirectory(repositoryPath);
        }
        var directories = Directory.GetDirectories(repositoryPath);
        var serverNames = new List<string>();
        foreach (var directory in directories)
        {
            var folderName = Path.GetFileName(directory);
            if (!string.IsNullOrEmpty(folderName))
            {
                serverNames.Add(folderName);
            }
        }
        return Task.FromResult(serverNames);
    }

    public async Task SaveServer(DatabaseInfo databaseInfo)
    {
        // Create a folder with the name of database in the repository folder
        var folderName = Path.Combine(REPOSITORY_FOLDER, databaseInfo.Name);
        if (!Directory.Exists(folderName))
        {
            Directory.CreateDirectory(folderName);
        }

        // Save the database info to json file (manifest.json)
        var filePath = Path.Combine(folderName, MANIFEST_FILE);
        var json = System.Text.Json.JsonSerializer.Serialize(databaseInfo);
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task<DatabaseInfo> LoadServer(string name)
    {
        // Load the database info from json file (manifest.json)
        var filePath = Path.Combine(REPOSITORY_FOLDER, name, MANIFEST_FILE);
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Database info for '{name}' not found.");
        }
        var json = await File.ReadAllTextAsync(filePath);
        var databaseInfo = System.Text.Json.JsonSerializer.Deserialize<DatabaseInfo>(json);
        if (databaseInfo == null)
        {
            throw new InvalidOperationException("Failed to deserialize database info.");
        }

        return databaseInfo;
    }
}
