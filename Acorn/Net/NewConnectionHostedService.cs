using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Acorn.Database.Models;
using Acorn.Database.Repository;
using Acorn.Infrastructure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Acorn.Net;

public class NewConnectionHostedService(
    IServiceProvider services,
    ILogger<NewConnectionHostedService> logger,
    ILogger<PlayerConnection> playerConnectionLogger,
    IStatsReporter statsReporter,
    WorldState worldState,
    IDbRepository<Character> characterRepository,
    ISessionGenerator sessionGenerator,
    IOptions<ServerOptions> serverOptions
) : IHostedService, IDisposable
{
    private readonly IDbRepository<Character> _characterRepository = characterRepository;
    private readonly TcpListener _listener = new(IPAddress.Any, serverOptions.Value.Port);
    private readonly ILogger<NewConnectionHostedService> _logger = logger;
    private readonly ILogger<PlayerConnection> _playerConnectionLogger = playerConnectionLogger;
    private readonly IServiceProvider _services = services;
    private readonly IStatsReporter _statsReporter = statsReporter;
    private readonly WorldState _world = worldState;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _statsReporter.Report();
        _listener.Start();
        _logger.LogInformation("Waiting for connections on {Endpoint}...", _listener.LocalEndpoint);
        while (true)
        {
            var client = _listener.AcceptTcpClient();
            if (client == null)
            {
                _logger.LogWarning("Accepted client was null, skipping...");
                continue;
            }

            _world.Players.Add(new PlayerConnection(_services, client, _playerConnectionLogger,
                async playerConnection =>
                {
                    if (playerConnection.Character is not null)
                    {
                        var map = _world.Maps.First(x => x.Id == playerConnection.Character.Map);
                        await map.Leave(playerConnection);
                        await _characterRepository.UpdateAsync(playerConnection.Character);
                    }

                    _world.Players = new ConcurrentBag<PlayerConnection>(
                        _world.Players.Where(x => x != playerConnection)
                    );

                    _logger.LogInformation("Player disconnected");
                    UpdateConnectedCount();
                }, sessionGenerator));

            _logger.LogInformation("Connection accepted. {PlayersConnected} players connected", _world.Players.Count);
            UpdateConnectedCount();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Dispose();
        return Task.CompletedTask;
    }

    private void UpdateConnectedCount()
    {
        Console.Title = $"Acorn Server ({_world.Players.Count} Connected)";
    }
    
    public void Dispose()
    {
        _listener.Stop();
        _listener.Dispose();
    }
}