using Acorn.Data.Repository;
using Acorn.Net.Models;
using Moffat.EndlessOnline.SDK.Protocol;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using OneOf;
using OneOf.Types;

namespace Acorn.Net.PacketHandlers;
internal class WelcomeMsgClientPacketHandler : IPacketHandler<WelcomeMsgClientPacket>
{
    private readonly IDataFileRepository _dataRepository;

    public WelcomeMsgClientPacketHandler(
        IDataFileRepository dataRepository
    )
    {
        _dataRepository = dataRepository;
    }

    public async Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, WelcomeMsgClientPacket packet)
    {
        playerConnection.ClientState = ClientState.InGame;

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
                    Characters = new List<CharacterMapInfo>()
                    {
                        new CharacterMapInfo
                        {
                            Name = playerConnection.Character.Name,
                            Coords = new BigCoords
                            {
                                X = playerConnection.Character.X,
                                Y = playerConnection.Character.Y
                            },
                            Direction = playerConnection.Character.Direction,
                            ClassId = playerConnection.Character.Class,
                            HairColor = playerConnection.Character.HairColor,
                            HairStyle = playerConnection.Character.HairStyle,
                            Gender = (Gender)playerConnection.Character.Gender,
                            Hp = playerConnection.Character.Hp,
                            Tp = playerConnection.Character.Tp,
                            MaxHp = playerConnection.Character.MaxHp,
                            MaxTp = playerConnection.Character.MaxTp,
                            Level = playerConnection.Character.Level,
                            MapId = playerConnection.Character.Map,
                            SitState = playerConnection.Character.SitState,
                            Skin = playerConnection.Character.Race,
                            PlayerId = playerConnection.SessionId,
                            GuildTag = "DAN",
                            Equipment = new(),
                            Invisible = playerConnection.Character.Hidden,
                            WarpEffect = WarpEffect.Admin
                        }
                    },
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
