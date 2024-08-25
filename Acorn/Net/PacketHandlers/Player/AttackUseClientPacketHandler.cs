using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using OneOf;
using OneOf.Types;

namespace Acorn.Net.PacketHandlers.Player;
internal class AttackUseClientPacketHandler : IPacketHandler<AttackUseClientPacket>
{
    private readonly WorldState _world;
    private readonly UtcNowDelegate _now;
    private DateTime _timeSinceLastAttack;

    public AttackUseClientPacketHandler(WorldState world, UtcNowDelegate now)
    {
        _world = world;
        _now = now;
    }

    public async Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, AttackUseClientPacket packet)
    {
        if ((_now() - _timeSinceLastAttack).TotalMilliseconds < 500)
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
