using Microsoft.Extensions.Logging;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using OneOf;
using OneOf.Types;

namespace Acorn.Net.PacketHandlers.Player;
public class ConnectionAcceptClientPacketHandler(
    ILogger<ConnectionAcceptClientPacketHandler> logger
) : IPacketHandler<ConnectionAcceptClientPacket>
{
    private readonly ILogger<ConnectionAcceptClientPacketHandler> _logger = logger;

    public async Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, ConnectionAcceptClientPacket packet)
    {
        if (playerConnection.Id != packet.PlayerId)
        {
            _logger.LogError("Mismatch PlayerId. Got {Actual} from packet but expected to be {Expected} from server records. Dropping connection.", packet.PlayerId, playerConnection.Id);
            return new Error();
        }

        _logger.LogDebug("Got expected connection accept packet from {Location} for player id {PlayerId}", playerConnection.TcpClient.Client.RemoteEndPoint, playerConnection.Id);
        return new Success();
    }

    public Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, object packet)
        => HandleAsync(playerConnection, (ConnectionAcceptClientPacket)packet);
}
