using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using OneOf;
using OneOf.Types;

namespace Acorn.Net.PacketHandlers.Player;

public class RefreshRequestClientPacketHandler : IPacketHandler<RefreshRequestClientPacket>
{
    private readonly WorldState _world;

    public RefreshRequestClientPacketHandler(WorldState world)
    {
        _world = world;
    }

    public async Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection,
        RefreshRequestClientPacket packet)
    {
        await playerConnection.Send(new RefreshReplyServerPacket
        {
            Nearby = _world.MapFor(playerConnection).AsNearbyInfo()
        });
        return new Success();
    }

    public Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, object packet)
    {
        return HandleAsync(playerConnection, (RefreshRequestClientPacket)packet);
    }
}