using Acorn.Data.Repository;
using Acorn.Net;
using Moffat.EndlessOnline.SDK.Protocol.Map;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Moffat.EndlessOnline.SDK.Protocol.Pub;
using System.Collections.Concurrent;
using System.Data;

namespace Acorn;

public class WorldState
{
    public ConcurrentBag<PlayerConnection> Players = [];
    public ConcurrentBag<MapState> Maps = [];
    public ConcurrentBag<GlobalMessage> GlobalMessages = [];

    public WorldState(IDataFileRepository dataRepository)
    {
        foreach (var map in dataRepository.Maps)
        {
            Maps.Add(new MapState(map));
        }
    }

    public MapState MapFor(PlayerConnection player)
        => Maps.Single(x => x.HasPlayer(player));

    public Task Refresh(PlayerConnection player) => Warp(player, player.Character.Map, player.Character.X, player.Character.Y);

    public async Task Warp(PlayerConnection player, int mapId, int x, int y, WarpEffect warpEffect = WarpEffect.None, bool localWarp = true)
    {
        player.WarpSession = new()
        {
            WarpEffect = warpEffect,
            Local = localWarp,
            MapId = mapId,
            X = x,
            Y = y
        };

        await player.WarpSession.Begin(player, this);

        if (!localWarp)
        {
            var currentMap = MapFor(player);
            var newMap = Maps.Single(x => x.Id == mapId);
            await currentMap.Leave(player, warpEffect);
            await newMap.Enter(player, warpEffect);
        }
    }
}

public record GlobalMessage(
    Guid Id, 
    string Message,
    string Author,
    DateTime CreatedAt
)
{
    public static GlobalMessage Welcome() => 
        new(
            Guid.NewGuid(), 
            "Welcome to Acorn! Please be respectful.", 
            "Server", 
            DateTime.UtcNow
        );
}

public class MapState
{
    public int Id { get; set; }
    public Emf Data { get; set; }

    public ConcurrentBag<NpcState> Npcs { get; set; } = new();
    public ConcurrentBag<PlayerConnection> Players { get; set; } = new();

    public MapState(MapWithId data)
    {
        Id = data.Id;
        Data = data.Map;
    }

    public bool HasPlayer(PlayerConnection player)
    {
        return Players.Contains(player);
    }

    public IEnumerable<PlayerConnection> PlayersExcept(PlayerConnection playerConnection) =>
        Players.Where(x => x != playerConnection);

    public async Task BroadcastPacket(IPacket packet, PlayerConnection? except = null)
    {
        var otherPlayers = Players.Where(x => except is null || x != except)
            .ToList();

        var broadcast = otherPlayers
            .Select(async otherPlayer => await otherPlayer.Send(packet));

        await Task.WhenAll(broadcast);
    }

    public NearbyInfo AsNearbyInfo(PlayerConnection? except = null, WarpEffect warpEffect = WarpEffect.None) => new()
    {
        Characters = Players
            .Where(x => x.Character is not null)
            .Where(x => except == null || x != except)
            .Select(x => x.Character.AsCharacterMapInfo(x.SessionId, warpEffect))
            .ToList(),
        Items = [],
        Npcs = []
    };

    public async Task Enter(PlayerConnection player, WarpEffect warpEffect = WarpEffect.None)
    {
        player.Character.Map = Id;

        if (!Players.Contains(player))
            Players.Add(player);

        await BroadcastPacket(new PlayersAgreeServerPacket
        {
            Nearby = AsNearbyInfo(null, warpEffect)
        }, except: player);
    }

    public async Task Leave(PlayerConnection player, WarpEffect warpEffect = WarpEffect.None)
    {
        Players = new ConcurrentBag<PlayerConnection>(Players.Where(p => p != player));

        await BroadcastPacket(new PlayersRemoveServerPacket
        {
            PlayerId = player.SessionId,
        });

        await BroadcastPacket(new AvatarRemoveServerPacket
        {
            PlayerId = player.SessionId,
            WarpEffect = warpEffect
        });
    }

    public void Tick()
    {
        foreach (var player in Players)
        {
            var before = player.Character.Hp;
            var after = player.Character.Recover(15);
        }
    }
}

public class NpcState
{
    public EnfRecord Data { get; set; }

    public int Hp { get; set; }

    public NpcState(EnfRecord data)
    {
        Data = data;
    }
}