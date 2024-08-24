using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using OneOf;
using OneOf.Types;

namespace Acorn.Net.PacketHandlers.Player;
public class PlayerRangeRequestClientPacketHandler : IPacketHandler<PlayerRangeRequestClientPacket>
{
    public async Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, PlayerRangeRequestClientPacket packet)
    {
        return new Success();
    }

    public Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, object packet)
        => HandleAsync(playerConnection, (PlayerRangeRequestClientPacket)packet);
}
