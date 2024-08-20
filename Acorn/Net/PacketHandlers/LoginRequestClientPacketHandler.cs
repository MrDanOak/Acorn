using Acorn.Data.Models;
using Acorn.Data.Repository;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using OneOf;
using OneOf.Types;

namespace Acorn.Net.PacketHandlers;
public class LoginRequestClientPacketHandler(
    ILogger<LoginRequestClientPacketHandler> logger,
    IRepository<Account> repository,
    IMapper mapper
) : IPacketHandler<LoginRequestClientPacket>
{
    private readonly ILogger<LoginRequestClientPacketHandler> _logger = logger;
    private readonly IRepository<Account> _repository = repository;
    private readonly IMapper _mapper = mapper;

    public async Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, LoginRequestClientPacket packet)
    {
        var responsePacket = (await _repository.GetByKey(packet.Username))
            .Match(success =>
            {
                playerConnection.CurrentPlayer = success.Value;
                return new LoginReplyServerPacket
                {
                    ReplyCode = LoginReply.Ok,
                    ReplyCodeData = new LoginReplyServerPacket.ReplyCodeDataOk()
                    {
                        Characters = playerConnection.CurrentPlayer.Characters.Select((x, id) =>
                        {
                            var entry = _mapper.Map<CharacterSelectionListEntry>(x);
                            entry.Id = id;
                            return entry;
                        }).ToList()
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
