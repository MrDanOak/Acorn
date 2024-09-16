using Acorn.Data.Repository;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Moffat.EndlessOnline.SDK.Protocol.Pub;

namespace Acorn.Net.PacketHandlers.Player.Talk;
public class SpawnNpcCommandHandler : ITalkHandler
{
    private WorldState _world;
    private IDataFileRepository _dataFiles;

    public SpawnNpcCommandHandler(WorldState world, IDataFileRepository dataFiles)
    {
        _world = world;
        _dataFiles = dataFiles;
    }

    public bool CanHandle(string command) =>
        command.Equals("spawnnpc", StringComparison.InvariantCultureIgnoreCase)
        || command.Equals("snpc", StringComparison.InvariantCultureIgnoreCase);

    public Task HandleAsync(PlayerConnection playerConnection, string command, params string[] args)
    {
        if (args.Length < 1)
        {
            return playerConnection.Send(new TalkServerServerPacket
            {
                Message = "Usage: $spawnnpc <npc_id|npc_name>"
            });
        }

        if (!int.TryParse(args[0], out var npcId))
        {
            return SpawnByName(playerConnection, args[0]);
        }

        _dataFiles.Enf.GetNpc(npcId).Switch(
            async npc =>
            {
                await SpawnNpc(playerConnection, npc.Value);
            },
            async error =>
            {
                await playerConnection.Send(new TalkServerServerPacket
                {
                    Message = error.Value
                });
            }
        );


        return Task.CompletedTask;
    }

    private async Task SpawnNpc(PlayerConnection playerConnection, EnfRecord enf)
    {
        var npcId = _dataFiles.Enf.Npcs.FindIndex(x => enf.GetHashCode() == x.GetHashCode());

        var npc = new NpcState(enf)
        {
            Direction = playerConnection.Character.Direction,
            X = playerConnection.Character.X,
            Y = playerConnection.Character.Y,
            Id = npcId + 1
        };

        var playerMap = _world.MapFor(playerConnection);

        playerMap.Npcs.Add(npc);
        await playerConnection.Send(new TalkServerServerPacket
        {
            Message = $"Spawned NPC {enf.Name} ({npcId})."
        });

        await playerMap.BroadcastPacket(new NpcAgreeServerPacket
        {
            Npcs = playerMap.AsNpcMapInfo()
        });
    }

    private Task SpawnByName(PlayerConnection playerConnection, string name)
    {
        var npc = _dataFiles.Enf.Npcs.FirstOrDefault(x => x.Name.Contains(name, StringComparison.CurrentCultureIgnoreCase));

        if (npc == null)
        {
            return playerConnection.Send(new TalkServerServerPacket
            {
                Message = $"NPC {name} not found."
            });
        }

        return SpawnNpc(playerConnection, npc);
    }
}
