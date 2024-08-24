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

        map.Enter(playerConnection);

        var characters = map.Players
            .Where(x => x.Character != null)
            .Select(x => new CharacterMapInfo
            {
                Name = x.Character.Name,
                Coords = new BigCoords
                {
                    X = x.Character.X,
                    Y = x.Character.Y
                },
                Direction = x.Character.Direction,
                ClassId = x.Character.Class,
                HairColor = x.Character.HairColor,
                HairStyle = x.Character.HairStyle,
                Gender = x.Character.Gender,
                Hp = x.Character.Hp,
                Tp = x.Character.Tp,
                MaxHp = x.Character.MaxHp,
                MaxTp = x.Character.MaxTp,
                Level = x.Character.Level,
                MapId = x.Character.Map,
                SitState = x.Character.SitState,
                Skin = x.Character.Race,
                PlayerId = x.SessionId,
                Equipment = new(),
                Invisible = x.Character.Hidden,
                WarpEffect = WarpEffect.None,
                GuildTag = "   "
            }).ToList();

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
                    Characters = characters,
                    Items = [],
                    Npcs = []
                }
            }
        });
        return new Success();
    }

    public Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, object packet)
        => HandleAsync(playerConnection, (WelcomeMsgClientPacket)packet);
}
