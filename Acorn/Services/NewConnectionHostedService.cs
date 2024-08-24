using Acorn.Data;
using Acorn.Data.Repository;
using Acorn.Infrastructure;
using Acorn.Net;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace Acorn.Services;

public class NewConnectionHostedService(
    IServiceProvider services,
    ILogger<NewConnectionHostedService> logger,
    ILogger<PlayerConnection> playerConnectionLogger,
    IStatsReporter statsReporter,
    WorldState worldState,
    IDbRepository<Character> characterRepository
) : IHostedService, IDisposable
{
    private readonly TcpListener _listener = new(IPAddress.Loopback, 8078);
    private readonly ILogger<NewConnectionHostedService> _logger = logger;
    private readonly ILogger<PlayerConnection> _playerConnectionLogger = playerConnectionLogger;
    private readonly IServiceProvider _services = services;
    private readonly IStatsReporter _statsReporter = statsReporter;
    private readonly WorldState _world = worldState;
    private readonly IDbRepository<Character> _characterRepository = characterRepository;

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

            _world.Players.Add(new PlayerConnection(_services, client, _playerConnectionLogger, async handler =>
            {
                _world.Players = new ConcurrentBag<PlayerConnection>(
                    _world.Players.Where(x => x != handler)
                );

                if (handler.Character is not null)
                {
                    await _characterRepository.UpdateAsync(handler.Character);
                }
                _logger.LogInformation("Player disconnected");
                UpdateConnectedCount();
            }));

            _logger.LogInformation("Connection accepted. {PlayersConnected} players connected", _world.Players.Count);
            UpdateConnectedCount();
        }
    }

    private void UpdateConnectedCount()
    {
        Console.Title = $"Acorn Server ({_world.Players.Count} Connected)";
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Dispose();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _listener.Dispose();
    }
}