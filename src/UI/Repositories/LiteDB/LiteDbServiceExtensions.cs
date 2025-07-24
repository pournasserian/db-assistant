namespace UI.Repositories.LiteDB;

public static class LiteDbServiceExtensions
{
    public static IServiceCollection AddLiteDbRepositories(this IServiceCollection services, string connectionString)
    {
        // Register LiteDB context and options
        services.AddSingleton(provider => CreateLiteDBOptions(provider, connectionString));
        services.AddSingleton<ILiteDBContext, LiteDBContext>();

        // Register repositories
      

        return services;
    }

    private static LiteDBOptions<LiteDBContext> CreateLiteDBOptions(IServiceProvider provider, string connectionString)
    {
        var configuration = provider.GetService<IConfiguration>() ?? throw new InvalidOperationException("IConfiguration is not registered.");
        var connString = configuration.GetConnectionString(connectionString);
        return connString is not null
            ? new LiteDBOptions<LiteDBContext>(connString)
            : throw new InvalidOperationException($"Connection string '{connectionString}' not found.");
    }
}