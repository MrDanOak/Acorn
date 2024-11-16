using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using OneOf;
using OneOf.Types;

namespace Acorn.Net.PacketHandlers.Player;

public class GlobalOpenClientPacketHandler : IPacketHandler<GlobalOpenClientPacket>
{
    private readonly WorldState _world;

    public GlobalOpenClientPacketHandler(WorldState world)
    {
        _world = world;
    }

    public Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, GlobalOpenClientPacket packet)
    {
        playerConnection.IsListeningToGlobal = true;
        new[] { GlobalMessage.Welcome() }
            .Concat(_world.GlobalMessages.OrderByDescending(x => x.CreatedAt).Take(10))
            .ToList()
            .ForEach(async x =>
            {
                await playerConnection.Send(new TalkMsgServerPacket
                {
                    Message = x.Message,
                    PlayerName = x.Author
                });
            });

        return Task.FromResult(OneOf<Success, Error>.FromT0(new Success()));
    }

    public Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, object packet)
    {
        return HandleAsync(playerConnection, (GlobalOpenClientPacket)packet);
    }
}