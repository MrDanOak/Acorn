using Acorn.Data.Repository;
using Acorn.Services;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moffat.EndlessOnline.SDK.Protocol;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using OneOf;
using OneOf.Types;

namespace Acorn.Net.PacketHandlers;
internal class WelcomeRequestClientPacketHandler : IPacketHandler<WelcomeRequestClientPacket>
{
    private readonly ISessionGenerator _sessionGenerator;
    private readonly IDataRepository _dataRepository;
    private readonly ILogger<WelcomeRequestClientPacketHandler> _logger;
    private readonly IMapper _mapper;

    public WelcomeRequestClientPacketHandler(
        ISessionGenerator sessionGenerator,
        IDataRepository dataRepository,
        ILogger<WelcomeRequestClientPacketHandler> logger,
        IMapper mapper
    )
    {
        _sessionGenerator = sessionGenerator;
        _dataRepository = dataRepository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, WelcomeRequestClientPacket packet)
    {
        var character = playerConnection.CurrentPlayer?.Characters[packet.CharacterId];
        if (character is null)
        {
            _logger.LogError("Could not find character");
            return new Error();
        }

        playerConnection.SessionId = _sessionGenerator.Generate();
        var map = _dataRepository.Maps.FirstOrDefault(map => map.Id == character.Map)?.Map;
        if (map is null)
        {
            _logger.LogError("Could not find map {MapId} for character {Name}", character.Map, character.Name);
            return new Error();
        }

        var equipmentResult = character.Equipment();
        if (equipmentResult.IsT1)
        {
            _logger.LogError("Could not get equipment for character {Name}. Error: {Error}", character.Name, equipmentResult.AsT1.Value);
            return new Error();
        }

        playerConnection.Character = character;

        await playerConnection.Send(new WelcomeReplyServerPacket
        {
            WelcomeCode = WelcomeCode.SelectCharacter,
            WelcomeCodeData = new WelcomeReplyServerPacket.WelcomeCodeDataSelectCharacter
            {
                Admin = (AdminLevel)character.Admin,
                CharacterId = packet.CharacterId,
                ClassId = character.Class,
                EcfLength = _dataRepository.Ecf.ByteSize,
                EcfRid = _dataRepository.Ecf.Rid,
                EifLength = _dataRepository.Eif.ByteSize,
                EifRid = _dataRepository.Eif.Rid,
                EnfLength = _dataRepository.Enf.ByteSize,
                EnfRid = _dataRepository.Enf.Rid,
                Equipment = _mapper.Map<EquipmentWelcome>(equipmentResult.AsT0.Value),
                EsfLength = _dataRepository.Esf.ByteSize,
                EsfRid = _dataRepository.Esf.Rid,
                Experience = character.Exp,
                GuildName = character.Guild ?? "",
                GuildRank = character.GuildRank,
                GuildRankName = character.GuildRankString ?? "",
                GuildTag = character.Guild ?? "   ",
                MapFileSize = map.ByteSize,
                MapId = character.Map,
                MapRid = map.Rid,
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
