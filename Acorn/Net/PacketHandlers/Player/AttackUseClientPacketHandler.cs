using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using OneOf;
using OneOf.Types;
using System.Diagnostics;

namespace Acorn.Net.PacketHandlers.Player;
internal class AttackUseClientPacketHandler : IPacketHandler<AttackUseClientPacket>
{
    private WorldState _world;
    private DateTime _timeSinceLastAttack;

    public AttackUseClientPacketHandler(WorldState world)
    {
        _world = world;
    }

    public async Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, AttackUseClientPacket packet)
    {
        if ((DateTime.UtcNow - _timeSinceLastAttack).TotalMilliseconds < 500)
        {
            return new Success();
        }

        var map = _world.MapFor(playerConnection);

        map.Players.Where(x => x != playerConnection).ToList().ForEach(async otherPlayer =>
        {
            await otherPlayer.Send(new AttackPlayerServerPacket
            {
                Direction = playerConnection.Character.Direction,
                PlayerId = playerConnection.SessionId
            });
        });

        _timeSinceLastAttack = DateTime.UtcNow;
        return new Success();
    }

    public Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, object packet)
         => HandleAsync(playerConnection, (AttackUseClientPacket)packet);
}
