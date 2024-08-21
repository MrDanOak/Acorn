using Acorn.Services;
using Moffat.EndlessOnline.SDK.Protocol;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using OneOf;
using OneOf.Types;

namespace Acorn.Net.PacketHandlers;
internal class WelcomeRequestClientPacketHandler : IPacketHandler<WelcomeRequestClientPacket>
{
    private readonly ISessionGenerator _sessionGenerator;

    public WelcomeRequestClientPacketHandler(ISessionGenerator sessionGenerator)
    {
        _sessionGenerator = sessionGenerator;
    }

    public async Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, WelcomeRequestClientPacket packet)
    {
        var character = playerConnection.CurrentPlayer?.Characters[packet.CharacterId];
        playerConnection.SessionId = _sessionGenerator.Generate();

        await playerConnection.Send(new WelcomeReplyServerPacket
        {
            WelcomeCode = WelcomeCode.SelectCharacter,
            WelcomeCodeData = new WelcomeReplyServerPacket.WelcomeCodeDataSelectCharacter
            {
                Admin = (AdminLevel)character?.Admin,
                CharacterId = packet.CharacterId,
                ClassId = character.Class,
                EcfLength = 0,
                EcfRid = [],
                EifLength = 0,
                EifRid = [],
                EnfLength = 0,
                EnfRid = [],
                Equipment = new EquipmentWelcome() { },
                EsfLength = 0,
                EsfRid = [],
                Experience = character.Exp,
                GuildName = character.Guild,
                GuildRank = character.GuildRank,
                GuildRankName = character.GuildRankString,
                GuildTag = character.Guild,
                MapFileSize = 0,
                MapId = character.Map,
                MapRid = [],
                Name = character.Name,
                SessionId = playerConnection.SessionId,
                Level = character.Level,
                LoginMessageCode = character.Usage switch
                {
                    0 => LoginMessageCode.Yes,
                    _ => LoginMessageCode.No
                },
                Settings = new ServerSettings()
                {
                    JailMap = 2,
                    RescueMap = 4,
                    RescueCoords = new Coords { X = 24, Y = 24 },
                    SpyAndLightGuideFloodRate = 10,
                    GuardianFloodRate = 10,
                    GameMasterFloodRate = 10,
                    HighGameMasterFloodRate = 0
                }
            }
        });

        return new Success();
    }

    public Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, object packet)
        => HandleAsync(playerConnection, (WelcomeRequestClientPacket)packet);
}
