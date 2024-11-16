namespace Acorn;

public class ServerOptions
{
    public NewCharacterOptions NewCharacter { get; set; } = new();
    public required string ServerName { get; set; }
    public required string Site { get; set; }
    public required string Hostname { get; set; }
    public required int Port { get; set; }
}

public class NewCharacterOptions
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Map { get; set; }
}