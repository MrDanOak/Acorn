using Acorn.Data;
using Acorn.Data.Repository;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moffat.EndlessOnline.SDK.Protocol;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using OneOf;
using OneOf.Types;

namespace Acorn.Net.PacketHandlers.Character;
internal class CharacterCreateClientPacketHandler : IPacketHandler<CharacterCreateClientPacket>
{
    private readonly IDbRepository<Data.Character> _repository;
    private readonly ILogger<CharacterCreateClientPacketHandler> _logger;
    private readonly IMapper _mapper;

    public CharacterCreateClientPacketHandler(
        IDbRepository<Data.Character> repository,
        ILogger<CharacterCreateClientPacketHandler> logger,
        IMapper mapper
    )
    {
        _repository = repository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, CharacterCreateClientPacket packet)
    {
        var characterQuery = await _repository.GetByKey(packet.Name);
        var exists = characterQuery.Match(success => true, notFound => false, err => false);

        if (exists)
        {
            await playerConnection.Send(
                new CharacterReplyServerPacket
                {
                    ReplyCode = CharacterReply.Exists,
                    ReplyCodeData = new CharacterReplyServerPacket.ReplyCodeDataExists()
                });

            return new Success();
        }

        var character = new Data.Character()
        {
            Name = packet.Name,
            Race = packet.Skin,
            Admin = packet.Name.ToLower() switch
            {
                "danzo" => (int)AdminLevel.HighGameMaster,
                _ => (int)AdminLevel.Player
            },
            Accounts_Username = playerConnection.Account?.Username ?? throw new InvalidOperationException("Cannot create a character without a user"),
            Map = 1,
            X = 1,
            Y = 1,
            HairColor = packet.HairColor,
            HairStyle = packet.HairStyle,
            Gender = (int)packet.Gender
        };

        await _repository.CreateAsync(character);
        _logger.LogInformation("Character '{Name}' created by '{Username}'.", character.Name, playerConnection.Account.Username);
        playerConnection.Account.Characters.Add(character);

        await playerConnection.Send(new CharacterReplyServerPacket
        {
            ReplyCode = CharacterReply.Ok,
            ReplyCodeData = new CharacterReplyServerPacket.ReplyCodeDataOk
            {
                Characters = playerConnection.Account.Characters.Select((c, id) =>
                {
                    var entry = _mapper.Map<CharacterSelectionListEntry>(c);
                    entry.Id = id;
                    return entry;
                }).ToList()
            }
        });

        return new Success();
    }

    public Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, object packet)
        => HandleAsync(playerConnection, (CharacterCreateClientPacket)packet);

}
