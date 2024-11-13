using Acorn.Net.PacketHandlers.Player.Talk;
using Moffat.EndlessOnline.SDK.Protocol;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using OneOf;
using OneOf.Types;

namespace Acorn.Net.PacketHandlers.Player;
internal class TalkReportClientPacketHandler : IPacketHandler<TalkReportClientPacket>
{
    private WorldState _world;
    private readonly IEnumerable<ITalkHandler> _talkHandlers;

    public TalkReportClientPacketHandler(WorldState world, IEnumerable<ITalkHandler> talkHandlers)
    {
        _world = world;
        _talkHandlers = talkHandlers;
    }

    public async Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, TalkReportClientPacket packet)
    {
        var map = _world.MapFor(playerConnection);
        var author = playerConnection.Character;

        if (author?.Admin > AdminLevel.Player && packet.Message.StartsWith("$"))
        {
            var args = packet.Message.Split(" ");
            var command = args[0][1..];

            var handler = _talkHandlers.FirstOrDefault(x => x.CanHandle(command));
            if (handler is null)
                return new Success();

            await handler.HandleAsync(playerConnection, command, args[1..]);
            return new Success();
        }

        await map.BroadcastPacket(new TalkPlayerServerPacket
        {
            Message = packet.Message,
            PlayerId = playerConnection.SessionId
        }, playerConnection);
        return new Success();
    }

    public Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, object packet)
        => HandleAsync(playerConnection, (TalkReportClientPacket)packet);
}
