using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using OneOf;
using OneOf.Types;

namespace Acorn.Net.PacketHandlers.Player.Warp;
public class WarpAcceptClientPacketHandler : IPacketHandler<WarpAcceptClientPacket>
{
    private WorldState _world;

    public WarpAcceptClientPacketHandler(WorldState world)
    {
        _world = world;
    }

    public async Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, WarpAcceptClientPacket packet)
    {
        if (playerConnection.WarpSession == null)
        {
            return new Error();
        }

        if (playerConnection.WarpSession.Local)
        {
            await playerConnection.Send(new WarpAgreeServerPacket
            {
                Nearby = _world.MapFor(playerConnection).AsNearbyInfo(),
                WarpType = WarpType.Local
            });
        }

        return new Success();
    }

    public Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, object packet)
        => HandleAsync(playerConnection, (WarpAcceptClientPacket)packet);
}
