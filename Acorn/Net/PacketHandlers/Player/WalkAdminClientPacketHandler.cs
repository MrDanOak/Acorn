using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using OneOf;
using OneOf.Types;

namespace Acorn.Net.PacketHandlers.Player;
public class WalkAdminClientPacketHandler : IPacketHandler<WalkAdminClientPacket>
{
    private readonly IPacketHandler<WalkPlayerClientPacket> _playerWalkHandler;

    public WalkAdminClientPacketHandler(IPacketHandler<WalkPlayerClientPacket> playerWalkHandler)
    {
        _playerWalkHandler = playerWalkHandler;
    }

    public async Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, WalkAdminClientPacket packet)
    {
        return await _playerWalkHandler.HandleAsync(playerConnection, new WalkPlayerClientPacket
        {
            WalkAction = packet.WalkAction
        });
    }

    public Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, object packet)
        => HandleAsync(playerConnection, (WalkAdminClientPacket)packet);
}
