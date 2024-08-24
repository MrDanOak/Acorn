using Acorn;
using Acorn.Data;
using Acorn.Data.Repository;
using Acorn.Infrastructure;
using Acorn.Net.PacketHandlers;
using Acorn.Services;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;
using System.Reflection;

var GREEN = Console.IsOutputRedirected ? "" : "\x1b[92m";
var NORMAL = Console.IsOutputRedirected ? "" : "\x1b[39m";
var BOLD = Console.IsOutputRedirected ? "" : "\x1b[1m";
var NOBOLD = Console.IsOutputRedirected ? "" : "\x1b[22m";

Console.WriteLine($"""
{GREEN}          _       {BOLD}Acorn Endless-Online Server Software{NOBOLD}
        _/-\_     ------------------------------------
     .-`-:-:-`-.  {GREEN}Author:{NORMAL} Dan Oak{GREEN}
    /-:-:-:-:-:-\ {GREEN}Version:{NORMAL} 0.0.0.1{GREEN}
    \:-:-:-:-:-:/ 
     |`       `|  
     |         |  
     `\       /'  
       `-._.-'    {NORMAL}
""");

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// service collection builder
await Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services
            .AddAutoMapper(Assembly.GetExecutingAssembly())
            .AddSingleton<IConfiguration>(configuration)
            .Configure<DatabaseOptions>(configuration.GetSection("Database"))
            .AddSingleton<UtcNowDelegate>(() => DateTime.UtcNow)
            .AddTransient<IDbConnection>(sp => {
                var dbOptions = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
                var connectionString = dbOptions.ConnectionString;
                return (dbOptions.Engine ?? "").ToLower() switch
                {
                    "sqlite" => new SqliteConnection(connectionString),
                    _ => new SqlConnection(connectionString)
                };
            })
            .AddSingleton<IStatsReporter, StatsReporter>()
            .AddSingleton<ISessionGenerator, SessionGenerator>()
            .AddTransient<IDbInitialiser, DbInitialiser>()
            .AddHostedService<NewConnectionHostedService>()
            .AddHostedService<WorldHostedService>()
            .AddSingleton<WorldState>()
            .AddPacketHandlers()
            .AddRepositories();
    })
    .ConfigureLogging(builder =>
    {
        builder.SetMinimumLevel(LogLevel.Debug);
        builder.AddConsole(options =>
        {
            options.TimestampFormat = "[HH:mm:ss] ";
        });
    })
    .Build()
    .RunAsync();

static class IocRegistrations
{
    public static IServiceCollection AddPacketHandlers(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var packetHandlerType = typeof(IPacketHandler<>);

        var handlers = assembly.GetTypes()
            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == packetHandlerType))
            .ToList();

        foreach (var handler in handlers)
        {
            var interfaceType = handler.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == packetHandlerType);
            services.AddTransient(interfaceType, handler);
        }

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    => services
            .AddTransient<IDbRepository<Account>, AccountRepository>()
            .AddTransient<IDbRepository<Character>, CharacterRepository>()
            .AddSingleton<IDataFileRepository, DataFileRepository>()
        ;
}

public delegate DateTime UtcNowDelegate();