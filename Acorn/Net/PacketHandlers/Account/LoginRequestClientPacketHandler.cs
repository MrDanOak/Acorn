using Acorn.Data.Repository;
using Acorn.Extensions;
using Microsoft.Extensions.Logging;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using OneOf;
using OneOf.Types;

namespace Acorn.Net.PacketHandlers.Account;
public class LoginRequestClientPacketHandler(
    ILogger<LoginRequestClientPacketHandler> logger,
    IDbRepository<Database.Models.Account> repository,
    WorldState world
) : IPacketHandler<LoginRequestClientPacket>
{
    private readonly ILogger<LoginRequestClientPacketHandler> _logger = logger;
    private readonly IDbRepository<Database.Models.Account> _repository = repository;
    private readonly WorldState _world = world;

    public async Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, LoginRequestClientPacket packet)
    {
        var responsePacket = (await _repository.GetByKey(packet.Username))
            .Match(success =>
            {
                if (_world.Players.Any(x => string.Equals(x.Account?.Username, success.Value.Username, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return new LoginReplyServerPacket
                    {
                        ReplyCode = LoginReply.LoggedIn,
                        ReplyCodeData = new LoginReplyServerPacket.ReplyCodeDataLoggedIn { }
                    };
                }

                playerConnection.Account = success.Value;
                return new LoginReplyServerPacket
                {
                    ReplyCode = LoginReply.Ok,
                    ReplyCodeData = new LoginReplyServerPacket.ReplyCodeDataOk()
                    {
                        Characters = playerConnection.Account.Characters.Select((x, id) => x.AsCharacterListEntry(id)).ToList()
                    }
                };
            },
            notFound => new LoginReplyServerPacket
            {
                ReplyCode = LoginReply.WrongUser,
                ReplyCodeData = new LoginReplyServerPacket.ReplyCodeDataWrongUser()
            },
            error => new LoginReplyServerPacket
            {
                ReplyCode = LoginReply.Busy,
                ReplyCodeData = new LoginReplyServerPacket.ReplyCodeDataBusy()
            }
        );

        await playerConnection.Send(responsePacket);
        return new Success();
    }

    public Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, object packet) => HandleAsync(playerConnection, (LoginRequestClientPacket)packet);
}
