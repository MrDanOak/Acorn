using AutoMapper;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using OneOf;
using OneOf.Types;

namespace Acorn.Net.PacketHandlers;
internal class CharacterRequestClientPacketHandler : IPacketHandler<CharacterRequestClientPacket>
{
    private readonly IMapper _mapper;

    public CharacterRequestClientPacketHandler(IMapper mapper)
    {
        _mapper = mapper;
    }

    public async Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, CharacterRequestClientPacket packet)
    {
        if (string.Equals(packet.RequestString, "new", StringComparison.OrdinalIgnoreCase) is false)
        {
            return new Success();
        }

        if (playerConnection.CurrentPlayer?.Characters.Count() >= 3)
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
            ReplyCode = CharacterReply.Ok,
            ReplyCodeData = new CharacterReplyServerPacket.ReplyCodeDataOk
            {
                Characters = playerConnection.CurrentPlayer.Characters.Select((x, id) => {
                    var entry = _mapper.Map<CharacterSelectionListEntry>(x);
                    entry.Id = id;
                    return entry;
                }).ToList()
            }
        });

        return new Success();
    }

    public Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, object packet)
        => HandleAsync(playerConnection, (CharacterRequestClientPacket)packet);
}
