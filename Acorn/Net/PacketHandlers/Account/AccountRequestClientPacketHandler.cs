using Acorn.Data.Repository;
using Microsoft.Extensions.Logging;
using Moffat.EndlessOnline.SDK.Data;
using Moffat.EndlessOnline.SDK.Packet;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using OneOf;
using OneOf.Types;

namespace Acorn.Net.PacketHandlers.Account;
internal class AccountRequestClientPacketHandler(
    IDbRepository<Database.Models.Account> accountRepository,
    ILogger<AccountRequestClientPacket> logger
) : IPacketHandler<AccountRequestClientPacket>
{
    private readonly IDbRepository<Database.Models.Account> _accountRepository = accountRepository;
    private readonly ILogger<AccountRequestClientPacket> _logger = logger;

    public async Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, AccountRequestClientPacket packet)
    {
        (await _accountRepository.GetByKey(packet.Username)).Switch(async exists =>
        {
            _logger.LogDebug("Account exists");
            await playerConnection.Send(new AccountReplyServerPacket()
            {
                ReplyCode = AccountReply.Exists,
                ReplyCodeData = new AccountReplyServerPacket.ReplyCodeDataExists()
            });
        },
        async notFound =>
        {
            _logger.LogDebug("Account \"{Account}\" does not exist", packet.Username);

            if (playerConnection.StartSequence.Value > EoNumericLimits.CHAR_MAX)
                playerConnection.StartSequence = InitSequenceStart.Generate(playerConnection.Rnd);

            await playerConnection.Send(new AccountReplyServerPacket()
            {
                ReplyCode = (AccountReply)playerConnection.SessionId,
                ReplyCodeData = new AccountReplyServerPacket.ReplyCodeDataDefault()
                {
                    SequenceStart = playerConnection.StartSequence.Seq1
                }
            });
        },
        error =>
        {
            _logger.LogError("Account {Username} errored", packet.Username);
        });

        return new Success();
    }

    public Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, object packet)
        => HandleAsync(playerConnection, (AccountRequestClientPacket)packet);
}
