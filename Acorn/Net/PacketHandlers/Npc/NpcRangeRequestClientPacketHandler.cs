using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using OneOf;
using OneOf.Types;

namespace Acorn.Net.PacketHandlers.Npc;

public class NpcRangeRequestClientPacketHandler : IPacketHandler<NpcRangeRequestClientPacket>
{
    private readonly WorldState _world;

    public NpcRangeRequestClientPacketHandler(WorldState world)
    {
        _world = world;
    }

    public async Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection,
        NpcRangeRequestClientPacket packet)
    {
        var map = _world.MapFor(playerConnection);

        await playerConnection.Send(new NpcAgreeServerPacket
        {
            Npcs = map.AsNpcMapInfo()
        });

        return new Success();
    }

    public Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, object packet)
    {
        return HandleAsync(playerConnection, (NpcRangeRequestClientPacket)packet);
    }
}