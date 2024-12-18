﻿using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using OneOf;
using OneOf.Types;

namespace Acorn.Net.PacketHandlers.Player;

internal class FacePlayerClientPacketHandler : IPacketHandler<FacePlayerClientPacket>
{
    private readonly WorldState _world;

    public FacePlayerClientPacketHandler(WorldState world)
    {
        _world = world;
    }

    public async Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection,
        FacePlayerClientPacket packet)
    {
        if (playerConnection.Character is null)
        {
            return new Error();
        }

        playerConnection.Character.Direction = packet.Direction;

        var broadcast = _world.MapFor(playerConnection).Players.ToList().Select(player =>
            player.Send(new FacePlayerServerPacket
            {
                Direction = packet.Direction,
                PlayerId = playerConnection.SessionId
            }));

        await Task.WhenAll(broadcast);
        return new Success();
    }

    public Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, object packet)
    {
        return HandleAsync(playerConnection, (FacePlayerClientPacket)packet);
    }
}