using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using OneOf;
using OneOf.Types;

namespace Acorn.Net.PacketHandlers.Player;

internal class GlobalCloseClientPacketHandler : IPacketHandler<GlobalCloseClientPacket>
{
    public Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, GlobalCloseClientPacket packet)
    {
        playerConnection.IsListeningToGlobal = false;
        return Task.FromResult(OneOf<Success, Error>.FromT0(new Success()));
    }

    public Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, object packet)
    {
        return HandleAsync(playerConnection, (GlobalCloseClientPacket)packet);
    }
}