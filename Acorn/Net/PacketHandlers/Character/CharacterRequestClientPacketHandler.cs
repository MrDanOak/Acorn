using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using OneOf;
using OneOf.Types;

namespace Acorn.Net.PacketHandlers.Character;

internal class CharacterRequestClientPacketHandler : IPacketHandler<CharacterRequestClientPacket>
{
    public async Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection,
        CharacterRequestClientPacket packet)
    {
        if (string.Equals(packet.RequestString, "new", StringComparison.OrdinalIgnoreCase) is false)
        {
            return new Success();
        }

        if (playerConnection.Account?.Characters.Count() >= 3)
        {
            await playerConnection.Send(new CharacterReplyServerPacket
            {
                ReplyCode = CharacterReply.Full,
                ReplyCodeData = new CharacterReplyServerPacket.ReplyCodeDataFull()
            });
            return new Success();
        }

        await playerConnection.Send(new CharacterReplyServerPacket
        {
            ReplyCode = (CharacterReply)playerConnection.SessionId,
            ReplyCodeData = new CharacterReplyServerPacket.ReplyCodeDataDefault()
        });

        return new Success();
    }

    public Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, object packet)
    {
        return HandleAsync(playerConnection, (CharacterRequestClientPacket)packet);
    }
}