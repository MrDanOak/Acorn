using Microsoft.Extensions.Logging;
using Moffat.EndlessOnline.SDK.Protocol;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using OneOf;
using OneOf.Types;

namespace Acorn.Net.PacketHandlers.Player;
internal class WalkPlayerClientPacketHandler : IPacketHandler<WalkPlayerClientPacket>
{
    private readonly ILogger<WalkPlayerClientPacketHandler> _logger;
    private readonly WorldState _world;

    public WalkPlayerClientPacketHandler(ILogger<WalkPlayerClientPacketHandler> logger, WorldState world)
    {
        _logger = logger;
        _world = world;
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

        var map = _world.Maps.FirstOrDefault(x => x.Id == playerConnection.Character.Map);

        var otherPlayers = map.Players.Where(x => x.Character.Map == map.Id).ToList();

        otherPlayers.ForEach(player =>
        {
            player.Send(new WalkPlayerClientPacket
            {
            });
        });

        return new Success();
    }

    public Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, object packet)
        => HandleAsync(playerConnection, (WalkPlayerClientPacket)packet);
}
