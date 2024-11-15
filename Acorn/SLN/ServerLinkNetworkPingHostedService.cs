using System.Reflection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Refit;

namespace Acorn.SLN;

public class ServerLinkNetworkPingHostedService(
    ILogger<ServerLinkNetworkPingHostedService> logger, 
    IOptions<SLNOptions> slnOptions,
    IOptions<ServerOptions> serverOptions,
    IServerLinkNetworkClient client) : IHostedService
{
    private readonly ILogger<ServerLinkNetworkPingHostedService> _logger = logger;
    private readonly IServerLinkNetworkClient _client = client;
    private readonly SLNOptions _slnOptions = slnOptions.Value;
    private readonly ServerOptions _serverOptions = serverOptions.Value;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting ServerLinkNetworkPingHostedService");
        var timer = new PeriodicTimer(TimeSpan.FromMinutes(_slnOptions.PingRate));
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogDebug("Current assembly version {Version}", Assembly.GetExecutingAssembly().GetName()?.Version?.ToString());
                var response = await _client.GetSlnResponse(
                    "Acorn",
                    Assembly.GetExecutingAssembly().GetName()?.Version?.ToString() ?? throw new Exception("Could not get version of current assembly"),
                    _serverOptions.Hostname,
                    _serverOptions.Port,
                    _serverOptions.ServeName,
                    _serverOptions.Site,
                    _serverOptions.Zone,
                    0,
                    2
                );

                _logger.LogDebug("Response from SLN: {Response}", response);
                await timer.WaitForNextTickAsync(cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while getting sln response {Message}", e.Message);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Stopping ServerLinkNetworkPingHostedService");
        return Task.CompletedTask;
    }
}

public interface IServerLinkNetworkClient
{
    [Get("")]
    public Task<string> GetSlnResponse(
        string software, 
        string version, 
        string host, 
        int port, 
        string name, 
        string url, 
        string zone, 
        int clientMajorVersion, 
        int clientMinorVersion
    );
}