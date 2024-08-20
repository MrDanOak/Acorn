using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;

namespace Acorn.Net;

public class NewConnectionListener(IServiceProvider services, ILogger<NewConnectionListener> logger, ILogger<PlayerConnection> playerConnectionLogger) : IHostedService, IDisposable
{
    private readonly TcpListener _listener = new(IPAddress.Loopback, 8078);
    private readonly ILogger<NewConnectionListener> _logger = logger;
    private readonly ILogger<PlayerConnection> _playerConnectionLogger = playerConnectionLogger;
    private readonly IServiceProvider _services = services;

    public readonly List<PlayerConnection> PlayersConnected = [];

    public Task StartAsync(CancellationToken cancellationToken)
    {
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

            PlayersConnected.Add(new PlayerConnection(_services, client, _playerConnectionLogger, handler =>
            {
                PlayersConnected.Remove(handler);
                _logger.LogInformation("Player disconnected");
                UpdateConnectedCount();
            }));

            _logger.LogInformation("Connection accepted. {PlayersConnected} players connected", PlayersConnected.Count);
            UpdateConnectedCount();
        }
    }

    private void UpdateConnectedCount()
    {
        Console.Title = $"Acorn Server ({PlayersConnected.Count} Connected)";
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