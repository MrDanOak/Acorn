using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using OneOf;
using OneOf.Types;

namespace Acorn.Net.PacketHandlers.Player;
internal class AttackUseClientPacketHandler : IPacketHandler<AttackUseClientPacket>
{
    public async Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, AttackUseClientPacket packet)
    {
        return new Success();
    }

    public Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, object packet)
         => HandleAsync(playerConnection, (AttackUseClientPacket)packet);
}
