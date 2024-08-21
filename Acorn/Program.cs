using Acorn.Data.Models;
using Acorn.Data.Repository;
using Acorn.Net;
using Acorn.Net.PacketHandlers;
using Acorn.Services;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moffat.EndlessOnline.SDK.Protocol.Pub;
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

CheckSqliteIfUsing(configuration);

// service collection builder
await Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services
            .AddAutoMapper(Assembly.GetExecutingAssembly())
            .AddSingleton<IConfiguration>(configuration)
            .AddSingleton<UtcNowDelegate>(() => DateTime.UtcNow)
            .AddTransient<IDbConnection>(sp => configuration["Database:Engine"] switch
            {
                "SQLite" => new SqliteConnection(configuration.GetConnectionString("SQLite")),
                _ => new SqlConnection(configuration.GetConnectionString("MSSQL"))
            })
            .AddSingleton<IStatsReporter, StatsReporter>()
            .AddHostedService<NewConnectionListener>()
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

void CheckSqliteIfUsing(IConfiguration configuration)
{
    if (configuration["Database:Engine"]?.ToLower() != "sqlite")
    {
        return;
    }

    if (File.Exists(configuration.GetConnectionString("SQLite")))
    {
        return;
    }

    var sqliteConnection = new SqliteConnection(configuration.GetConnectionString("SQLite"));
    sqliteConnection.Open();
    using var command = sqliteConnection.CreateCommand();
    command.CommandText = File.ReadAllText("Database/SQLite/Init.sql");
    command.ExecuteNonQuery();
    sqliteConnection.Close();
}

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
            .AddTransient<Acorn.Data.Repository.MSSQL.AccountRepository>()
            .AddTransient<Acorn.Data.Repository.SQLite.AccountRepository>()
            .AddTransient<Acorn.Data.Repository.MSSQL.CharacterRepository>()
            .AddTransient<Acorn.Data.Repository.SQLite.CharacterRepository>()
            .AddTransient<IRepository<Account>>(provider =>
                provider.GetRequiredService<IConfiguration>()["Database:Engine"]?.ToLower() switch
                {
                    "sqlite" => provider.GetRequiredService<Acorn.Data.Repository.SQLite.AccountRepository>(),
                    _ => provider.GetRequiredService<Acorn.Data.Repository.MSSQL.AccountRepository>()
                })
            .AddTransient<IRepository<Character>>(provider =>
                provider.GetRequiredService<IConfiguration>()["Database:Engine"]?.ToLower() switch
                {
                    "sqlite" => provider.GetRequiredService<Acorn.Data.Repository.SQLite.CharacterRepository>(),
                    _ => provider.GetRequiredService<Acorn.Data.Repository.MSSQL.CharacterRepository>()
                })
            .AddSingleton<IDataRepository, DataRepository>()
        ;
}

public delegate DateTime UtcNowDelegate();