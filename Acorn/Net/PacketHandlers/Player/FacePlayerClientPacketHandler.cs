using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using OneOf;
using OneOf.Types;

namespace Acorn.Net.PacketHandlers.Player;
internal class FacePlayerClientPacketHandler : IPacketHandler<FacePlayerClientPacket>
{
    public async Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, FacePlayerClientPacket packet)
    {
        playerConnection.Character.Direction = packet.Direction;
        return new Success();
    }

    public Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, object packet)
        => HandleAsync(playerConnection, (FacePlayerClientPacket)packet);
}
