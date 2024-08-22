using Microsoft.Extensions.Logging;
using Moffat.EndlessOnline.SDK.Protocol;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using OneOf;
using OneOf.Types;

namespace Acorn.Net.PacketHandlers.Player;
internal class WalkPlayerClientPacketHandler : IPacketHandler<WalkPlayerClientPacket>
{
    private readonly ILogger<WalkPlayerClientPacketHandler> _logger;

    public WalkPlayerClientPacketHandler(ILogger<WalkPlayerClientPacketHandler> logger)
    {
        _logger = logger;
    }

    public async Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, WalkPlayerClientPacket packet)
    {
        playerConnection.Character.X = packet.WalkAction.Direction switch
        {
            Direction.Left => playerConnection.Character.X - 1,
            Direction.Right => playerConnection.Character.X + 1,
            _ => playerConnection.Character.X
        };
        playerConnection.Character.Y = packet.WalkAction.Direction switch
        {
            Direction.Up => playerConnection.Character.Y - 1,
            Direction.Down => playerConnection.Character.Y + 1,
            _ => playerConnection.Character.Y
        };

        //if (playerConnection.Character.X != packet.WalkAction.Coords.X || playerConnection.Character.Y != packet.WalkAction.Coords.Y)
        //{
        //    _logger.LogError("Expected character to have coords ({ExpX},{ExpY}) but found coords ({ActX},{ActY})",
        //        packet.WalkAction.Coords.X,
        //        packet.WalkAction.Coords.Y,
        //        playerConnection.Character.X,
        //        playerConnection.Character.Y
        //    );
        //    return new Error();
        //}

        return new Success();
    }

    public Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, object packet)
        => HandleAsync(playerConnection, (WalkPlayerClientPacket)packet);
}
