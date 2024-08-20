using Moffat.EndlessOnline.SDK.Data;
using Moffat.EndlessOnline.SDK.Packet;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using OneOf.Types;
using OneOf;
using Microsoft.Extensions.Logging;
using Acorn.Net.Models;

namespace Acorn.Net.PacketHandlers;

internal class InitInitClientPacketHandler(ILogger<InitInitClientPacketHandler> logger) : IPacketHandler<InitInitClientPacket>
{
    private readonly ILogger<InitInitClientPacketHandler> _logger = logger;

    public async Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, InitInitClientPacket packet)
    {
        playerConnection.PacketSequencer = playerConnection.PacketSequencer.WithSequenceStart(playerConnection.StartSequence);
        playerConnection.ClientEncryptionMulti = playerConnection.Rnd.Next(7) + 6;
        playerConnection.ServerEncryptionMulti = playerConnection.Rnd.Next(7) + 6;

        _logger.LogDebug("Sending Init Server Packet with Seq 1: {Seq1}, Seq 2: {Seq2} PlayerId: {PlayerId}", playerConnection.StartSequence.Seq1, playerConnection.StartSequence.Seq2, playerConnection.Id);
        await playerConnection.Send(new InitInitServerPacket
        {
            ReplyCode = InitReply.Ok,
            ReplyCodeData = new InitInitServerPacket.ReplyCodeDataOk
            {
                Seq1 = playerConnection.StartSequence.Seq1,
                Seq2 = playerConnection.StartSequence.Seq2,
                ClientEncryptionMultiple = playerConnection.ClientEncryptionMulti,
                ServerEncryptionMultiple = playerConnection.ServerEncryptionMulti,
                PlayerId = playerConnection.Id,
                ChallengeResponse = ServerVerifier.Hash(packet.Challenge)
            }
        });

        playerConnection.ClientState = ClientState.Initialized;

        return new Success();
    }

    public Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, object packet) => HandleAsync(playerConnection, (InitInitClientPacket)packet);
}
