﻿using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using OneOf;
using OneOf.Types;

namespace Acorn.Net.PacketHandlers.Player;
internal class ConnectionPingClientPacketHandler : IPacketHandler<ConnectionPingClientPacket>
{
    public Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, ConnectionPingClientPacket packet)
    {
        playerConnection.NeedPong = false;
        return Task.FromResult<OneOf<Success, Error>>(new Success());
    }

    public Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, object packet) => HandleAsync(playerConnection, (ConnectionPingClientPacket)packet);
}
