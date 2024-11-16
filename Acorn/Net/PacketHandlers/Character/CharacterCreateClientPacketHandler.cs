using Acorn.Database.Repository;
using Acorn.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moffat.EndlessOnline.SDK.Protocol;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using OneOf;
using OneOf.Types;

namespace Acorn.Net.PacketHandlers.Character;

internal class CharacterCreateClientPacketHandler(
    IDbRepository<Database.Models.Character> repository,
    ILogger<CharacterCreateClientPacketHandler> logger,
    IOptions<ServerOptions> gameOptions)
    : IPacketHandler<CharacterCreateClientPacket>
{
    private readonly ServerOptions _serverOptions = gameOptions.Value;
    
    public async Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection,
        CharacterCreateClientPacket packet)
    {
        var characterQuery = await repository.GetByKey(packet.Name);
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

        var character = new Database.Models.Character
        {
            Name = packet.Name,
            Race = packet.Skin,
            Admin = packet.Name.ToLower() switch
            {
                "danzo" => AdminLevel.HighGameMaster,
                _ => (int)AdminLevel.Player
            },
            Accounts_Username = playerConnection.Account?.Username ??
                throw new InvalidOperationException("Cannot create a character without a user"),
            Map = _serverOptions.NewCharacter.Map,
            X = _serverOptions.NewCharacter.X,
            Y = _serverOptions.NewCharacter.Y,
            HairColor = packet.HairColor,
            HairStyle = packet.HairStyle,
            Gender = packet.Gender
        };

        await repository.CreateAsync(character);
        logger.LogInformation("Character '{Name}' created by '{Username}'.", character.Name,
            playerConnection.Account.Username);
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
    {
        return HandleAsync(playerConnection, (CharacterCreateClientPacket)packet);
    }
}