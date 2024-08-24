using Acorn.Data.Repository;
using Acorn.Net.Models;
using Moffat.EndlessOnline.SDK.Protocol;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using OneOf;
using OneOf.Types;

namespace Acorn.Net.PacketHandlers.Player;
internal class WelcomeMsgClientPacketHandler : IPacketHandler<WelcomeMsgClientPacket>
{
    private readonly IDataFileRepository _dataRepository;
    private readonly WorldState _world;

    public WelcomeMsgClientPacketHandler(
        IDataFileRepository dataRepository,
        WorldState worldState
    )
    {
        _dataRepository = dataRepository;
        _world = worldState;
    }

    public async Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, WelcomeMsgClientPacket packet)
    {
        playerConnection.ClientState = ClientState.InGame;
        var map = _world.Maps.First(x => x.Id == playerConnection.Character.Map);

        var otherPlayersOnMap = map.Players
            .Where(x => x.Character != null)
            .ToList();

        var playersOnMapWithThisCharacater = otherPlayersOnMap
            .Concat(new List<PlayerConnection> { playerConnection })
            .ToList();

        map.Enter(playerConnection);

        await playerConnection.Send(new WelcomeReplyServerPacket()
        {
            WelcomeCode = WelcomeCode.EnterGame,
            WelcomeCodeData = new WelcomeReplyServerPacket.WelcomeCodeDataEnterGame()
            {
                Items = playerConnection.Character.Items().AsT0.Value.ToList(),
                News = ["Welcome to Acorn...", "", "", "", "", "", "", "", ""],
                Weight = new Weight
                {
                    Current = 0,
                    Max = 100
                },
                Nearby = new NearbyInfo
                {
                    Characters = playersOnMapWithThisCharacater
                        .Select(x => x.Character.AsCharacterMapInfo(x.SessionId)).ToList(),
                    Items = [],
                    Npcs = []
                }
            }
        });

        otherPlayersOnMap.ForEach(async otherPlayer =>
            await otherPlayer.Send(new PlayersAgreeServerPacket
            {
                Nearby = new NearbyInfo
                {
                    Characters = new List<CharacterMapInfo>
                    {
                        playerConnection.Character.AsCharacterMapInfo(playerConnection.SessionId)
                    }.Concat(otherPlayersOnMap.Where(x => x != otherPlayer).Select(x => x.Character.AsCharacterMapInfo(x.SessionId)))
                    .ToList(),
                    Items = [],
                    Npcs = []
                }
            })
        );
        return new Success();
    }

    public Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, object packet)
        => HandleAsync(playerConnection, (WelcomeMsgClientPacket)packet);
}
