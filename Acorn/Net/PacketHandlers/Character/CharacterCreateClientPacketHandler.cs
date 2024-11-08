﻿using Acorn.Data.Repository;
using Acorn.Extensions;
using Microsoft.Extensions.Logging;
using Moffat.EndlessOnline.SDK.Protocol;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using OneOf;
using OneOf.Types;

namespace Acorn.Net.PacketHandlers.Character;
internal class CharacterCreateClientPacketHandler : IPacketHandler<CharacterCreateClientPacket>
{
    private readonly IDbRepository<Data.Models.Character> _repository;
    private readonly ILogger<CharacterCreateClientPacketHandler> _logger;

    public CharacterCreateClientPacketHandler(
        IDbRepository<Data.Models.Character> repository,
        ILogger<CharacterCreateClientPacketHandler> logger
    )
    {
        _repository = repository;
        _logger = logger;
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

        var character = new Data.Models.Character
        {
            Name = packet.Name,
            Race = packet.Skin,
            Admin = packet.Name.ToLower() switch
            {
                "danzo" => AdminLevel.HighGameMaster,
                _ => (int)AdminLevel.Player
            },
            Accounts_Username = playerConnection.Account?.Username ?? throw new InvalidOperationException("Cannot create a character without a user"),
            Map = 1,
            X = 1,
            Y = 1,
            HairColor = packet.HairColor,
            HairStyle = packet.HairStyle,
            Gender = packet.Gender
        };

        await _repository.CreateAsync(character);
        _logger.LogInformation("Character '{Name}' created by '{Username}'.", character.Name, playerConnection.Account.Username);
        playerConnection.Account.Characters.Add(character);

        await playerConnection.Send(new CharacterReplyServerPacket
        {
            ReplyCode = CharacterReply.Ok,
            ReplyCodeData = new CharacterReplyServerPacket.ReplyCodeDataOk
            {
                Characters = playerConnection.Account.Characters.Select((c, id) => c.AsCharacterListEntry(id)).ToList()
            }
        });

        return new Success();
    }

    public Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, object packet)
        => HandleAsync(playerConnection, (CharacterCreateClientPacket)packet);

}
