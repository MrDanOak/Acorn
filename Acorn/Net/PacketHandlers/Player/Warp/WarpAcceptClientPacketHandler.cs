using Microsoft.Extensions.Logging;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using OneOf;
using OneOf.Types;

namespace Acorn.Net.PacketHandlers.Player.Warp;
public class WarpAcceptClientPacketHandler : IPacketHandler<WarpAcceptClientPacket>
{
    private WorldState _world;
    private readonly ILogger<WarpAcceptClientPacketHandler> _logger;

    public WarpAcceptClientPacketHandler(WorldState world, ILogger<WarpAcceptClientPacketHandler> logger)
    {
        _world = world;
        _logger = logger;
    }

    public async Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, WarpAcceptClientPacket packet)
    {
        if (playerConnection.WarpSession is null)
        {
            return new Error();
        }

        if (playerConnection.Character is null)
        {
            _logger.LogError("Player connection has no Character initialised.");
            return new Error();
        }

        //todo: cancel any trades and whatnot if in progress

        var currentMap = _world.MapFor(playerConnection);
        await currentMap.Leave(playerConnection, playerConnection.WarpSession.WarpEffect);

        playerConnection.Character.Map = playerConnection.WarpSession.MapId;
        playerConnection.Character.X = playerConnection.WarpSession.X;
        playerConnection.Character.Y = playerConnection.WarpSession.Y;
        playerConnection.Character.SitState = SitState.Stand;

        if (playerConnection.WarpSession.Local)
        {
            await playerConnection.Send(new WarpAgreeServerPacket
            {
                Nearby = currentMap.AsNearbyInfo(),
                WarpType = WarpType.Local
            });

            await currentMap
                .Enter(playerConnection, playerConnection.WarpSession.WarpEffect);

            return new Success();
        }

        var newMap = _world.Maps.Single(x => x.Id == playerConnection.Character.Map);

        await newMap.Enter(playerConnection, playerConnection.WarpSession.WarpEffect);

        await playerConnection.Send(new WarpAgreeServerPacket
        {
            Nearby = newMap.AsNearbyInfo(),
            WarpType = WarpType.MapSwitch,
            WarpTypeData = new WarpAgreeServerPacket.WarpTypeDataMapSwitch
            {
                MapId = playerConnection.Character.Map,
                WarpEffect = playerConnection.WarpSession.WarpEffect
            }
        });

        playerConnection.WarpSession = null;
        return new Success();
    }

    public Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, object packet)
        => HandleAsync(playerConnection, (WarpAcceptClientPacket)packet);
}
