using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace Acorn.Net.PacketHandlers.Player.Warp;
public class WarpSession
{
    public WarpEffect WarpEffect { get; set; }
    public bool Local { get; set; }
    public int MapId { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
}