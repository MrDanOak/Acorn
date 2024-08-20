using AutoMapper;
using Acorn.Data.Models;
using Acorn.Data.Repository;
using Acorn.Security;
using Microsoft.Extensions.Logging;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using OneOf;
using OneOf.Types;
using System.Text;

namespace Acorn.Net.PacketHandlers;
internal class AccountCreateClientPacketHandler(
    IRepository<Account> accountRepository,
    IMapper mapper,
    ILogger<AccountCreateClientPacketHandler> logger
) : IPacketHandler<AccountCreateClientPacket>
{
    private readonly IRepository<Account> _accountRepository = accountRepository;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<AccountCreateClientPacketHandler> _logger = logger;

    public async Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, AccountCreateClientPacket packet)
    {
        var exists = (await _accountRepository.GetByKey(packet.Username))
            .Match(_ => true, _ => false, _ => false);

        if (exists)
        {
            _logger.LogDebug("Account with username {Username} already exists...", packet.Username);
            await playerConnection.Send(
                new AccountReplyServerPacket()
                {
                    ReplyCode = AccountReply.Exists,
                    ReplyCodeData = new AccountReplyServerPacket.ReplyCodeDataExists()
                });

            return new Success();
        }

        var account = _mapper.Map<Account>(packet);
        account.Password = Hash.HashPassword(packet.Username, packet.Password, out var salt);
        account.Salt = Encoding.UTF8.GetString(salt);

        var result = await _accountRepository.CreateAsync(account);

        result.Switch(async success =>
        {
            _logger.LogInformation("New account '{Username}'", packet.Username);
            await playerConnection.Send(new AccountReplyServerPacket()
            {
                ReplyCode = AccountReply.Created,
                ReplyCodeData = new AccountReplyServerPacket.ReplyCodeDataCreated()
            });
        },
        error => { });
        return result;
    }

    public Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, object packet)
        => HandleAsync(playerConnection, (AccountCreateClientPacket)packet);
}
