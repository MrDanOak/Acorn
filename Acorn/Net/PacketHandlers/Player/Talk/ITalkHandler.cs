namespace Acorn.Net.PacketHandlers.Player.Talk;

public interface ITalkHandler
{
    bool CanHandle(string command);
    Task HandleAsync(PlayerConnection playerConnection, string command, params string[] args);
}