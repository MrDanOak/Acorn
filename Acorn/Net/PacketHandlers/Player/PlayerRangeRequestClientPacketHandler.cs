using Acorn.Extensions;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using OneOf;
using OneOf.Types;

namespace Acorn.Net.PacketHandlers.Player;
public class PlayerRangeRequestClientPacketHandler : IPacketHandler<PlayerRangeRequestClientPacket>
{
    private WorldState _world;

    public PlayerRangeRequestClientPacketHandler(WorldState world)
    {
        _world = world;
    }

    public async Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, PlayerRangeRequestClientPacket packet)
    {
        var map = _world.MapFor(playerConnection);
        await playerConnection.Send(new PlayersListServerPacket
        {
            PlayersList = new PlayersList
            {
                Players = map.Players.Select(x => x.Character.AsOnlinePlayer()).ToList()
            }
        });

        return new Success();
    }

    public Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, object packet)
        => HandleAsync(playerConnection, (PlayerRangeRequestClientPacket)packet);
}
