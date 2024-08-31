using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using OneOf;
using OneOf.Types;

namespace Acorn.Net.PacketHandlers.Player.Talk;
internal class TalkMsgClientPacketHandler : IPacketHandler<TalkMsgClientPacket>
{
    private WorldState _world;

    public TalkMsgClientPacketHandler(WorldState world)
    {
        _world = world;
    }

    public async Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, TalkMsgClientPacket packet)
    {
        _world.GlobalMessages.Add(new GlobalMessage(Guid.NewGuid(), packet.Message, playerConnection.Character?.Name!, DateTime.UtcNow));
        
        var broadcast = _world.Players
            .Where(x => x != playerConnection && x.IsListeningToGlobal)
            .Select(x => x.Send(new TalkMsgServerPacket
            {
                Message = packet.Message,
                PlayerName = playerConnection.Character?.Name!
            }));

        await Task.WhenAll(broadcast);
        return new Success();
    }

    public Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, object packet)
        => HandleAsync(playerConnection, (TalkMsgClientPacket)packet);
}
