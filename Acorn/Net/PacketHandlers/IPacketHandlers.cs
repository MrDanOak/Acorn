using Moffat.EndlessOnline.SDK.Protocol.Net;
using OneOf;
using OneOf.Types;

namespace Acorn.Net.PacketHandlers;

public interface IHandler
{
    Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, object packet);
}

public interface IPacketHandler<TPacket> : IHandler where TPacket : IPacket
{
    Task<OneOf<Success, Error>> HandleAsync(PlayerConnection playerConnection, TPacket packet);
}
