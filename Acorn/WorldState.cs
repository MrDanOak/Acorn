using Acorn.Data.Repository;
using Acorn.Net;
using Acorn.Net.PacketHandlers.Player.Warp;
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

    public async Task Warp(PlayerConnection player, int mapId, int x, int y)
    {
        var currentMap = MapFor(player);
        var newMap = Maps.Single(x => x.Id == mapId);
        await currentMap.Leave(player);

        player.WarpSession = new WarpSession() { WarpEffect = WarpEffect.None, Local = true, MapId = mapId, X = x, Y = y };

        await newMap.Enter(player);
        await player.Send(new WarpRequestServerPacket
        {
            MapId = mapId,
            SessionId = player.SessionId,
            WarpType = WarpType.Local,
            WarpTypeData = new WarpRequestServerPacket.WarpTypeDataMapSwitch
            {
                MapFileSize = newMap.Data.ByteSize,
                MapRid = newMap.Data.Rid
            }
        });
    }
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
        var broadcast = Players.Where(x => except is null || x == except)
            .ToList()
            .Select(async otherPlayer => await otherPlayer.Send(packet));

        await Task.WhenAll(broadcast);
    }

    public NearbyInfo AsNearbyInfo(PlayerConnection? except = null, WarpEffect warpEffect = WarpEffect.None) => new ()
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

        Players.Add(player);
        await BroadcastPacket(new PlayersAgreeServerPacket
        {
            Nearby = AsNearbyInfo(player, warpEffect)
        }, player);
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