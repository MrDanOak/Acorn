using Acorn.Infrastructure;
using Microsoft.Extensions.Logging;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using OneOf;
using OneOf.Types;

namespace Acorn.Net.PacketHandlers.Player;
public class ConnectionAcceptClientPacketHandler(
    ILogger<ConnectionAcceptClientPacketHandler> logger,
    ISessionGenerator sessionGenerator
) : IPacketHandler<ConnectionAcceptClientPacket>
{
    private readonly ILogger<ConnectionAcceptClientPacketHandler> _logger = logger;
    private readonly ISessionGenerator _sessionGenerator = sessionGenerator;

    public Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, ConnectionAcceptClientPacket packet)
    {
        if (playerConnection.SessionId != packet.PlayerId)
        {
            _logger.LogError("Mismatch PlayerId. Got {Actual} from packet but expected to be {Expected} from server records. Dropping connection.", packet.PlayerId, playerConnection.SessionId);
            return Task.FromResult<OneOf<Success, Error>>(new Error());
        }

        //playerConnection.SessionId = _sessionGenerator.Generate();
        _logger.LogDebug("Got expected connection accept packet from {Location} for player id {PlayerId}", playerConnection.TcpClient.Client.RemoteEndPoint, playerConnection.SessionId);
        return Task.FromResult<OneOf<Success, Error>>(new Success());
    }

    public Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, object packet)
        => HandleAsync(playerConnection, (ConnectionAcceptClientPacket)packet);
}
