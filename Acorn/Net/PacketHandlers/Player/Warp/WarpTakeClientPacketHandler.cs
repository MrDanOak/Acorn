using Microsoft.Extensions.Logging;
using Moffat.EndlessOnline.SDK.Data;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using OneOf;
using OneOf.Types;

namespace Acorn.Net.PacketHandlers.Player.Warp;
internal class WarpTakeClientPacketHandler : IPacketHandler<WarpTakeClientPacket>
{
    private readonly WorldState _world;
    private readonly ILogger<WarpTakeClientPacketHandler> _logger;

    public WarpTakeClientPacketHandler(WorldState world, ILogger<WarpTakeClientPacketHandler> logger)
    {
        _world = world;
        _logger = logger;
    }

    public async Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, WarpTakeClientPacket packet)
    {
        if (packet.SessionId != playerConnection.SessionId)
        {
            _logger.LogError("Sesison ID was not as expected for player {Player}", playerConnection.Account?.Username);
            return new Error();
        }

        var map = _world.Maps.Single(x => x.Id == packet.MapId);
        var writer = new EoWriter();
        map.Data.Serialize(writer);

        await playerConnection.Send(new InitInitServerPacket
        {
            ReplyCode = InitReply.WarpMap,
            ReplyCodeData = new InitInitServerPacket.ReplyCodeDataWarpMap
            {
                MapFile = new MapFile()
                {
                    Content = writer.ToByteArray(),
                }
            }
        });

        return new Success();
    }

    public Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, object packet)
        => HandleAsync(playerConnection, (WarpTakeClientPacket)packet);
}
