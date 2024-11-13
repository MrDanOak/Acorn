using Acorn.Extensions;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using OneOf;
using OneOf.Types;
using static Moffat.EndlessOnline.SDK.Protocol.Net.Server.InitInitServerPacket;

namespace Acorn.Net.PacketHandlers.Player;
public class PlayersRequestClientPacketHandler : IPacketHandler<PlayersRequestClientPacket>
{
    private readonly WorldState _world;

    public PlayersRequestClientPacketHandler(WorldState world)
    {
        _world = world;
    }

    public async Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, PlayersRequestClientPacket packet)
    {
        await playerConnection.Send(new InitInitServerPacket
        {
            ReplyCode = InitReply.PlayersList,
            ReplyCodeData = new ReplyCodeDataPlayersList
            {
                PlayersList = new()
                {
                    Players = _world.Players.Select(x => x.Character?.AsOnlinePlayer()).ToList()
                }
            }
        });

        return new Success();
    }

    public Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, object packet)
        => HandleAsync(playerConnection, (PlayersRequestClientPacket)packet);
}
