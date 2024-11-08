using Acorn.Data.Repository;
using Microsoft.Extensions.Logging;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using OneOf;
using OneOf.Types;
using Acorn.Extensions;

namespace Acorn.Net.PacketHandlers.Account;
internal class AccountCreateClientPacketHandler(
    IDbRepository<Database.Models.Account> accountRepository,
    ILogger<AccountCreateClientPacketHandler> logger,
    UtcNowDelegate nowDelegate
) : IPacketHandler<AccountCreateClientPacket>
{
    private readonly IDbRepository<Database.Models.Account> _accountRepository = accountRepository;
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

        var account = packet.AsNewAccount(nowDelegate());
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
