using Acorn.Data.Repository;
using Acorn.Net;
using Moffat.EndlessOnline.SDK.Protocol.Map;
using Moffat.EndlessOnline.SDK.Protocol.Pub;
using System.Collections.Concurrent;
using System.Data;

namespace Acorn;

public class WorldState
{
    public ConcurrentBag<PlayerConnection> PlayerConnections = [];
    public ConcurrentBag<MapState> Maps = [];
    public WorldState(IDataFileRepository dataRepository)
    {
        foreach (var map in dataRepository.Maps)
        {
            Maps.Add(new MapState(map));
        }
    }

    public async Task Tick()
    {
        foreach (var map in Maps)
        {
            map.Tick();
        }
    }
}

public class MapState
{
    public int Id { get; set; }
    public Emf Data { get; set; }

    public ConcurrentBag<NpcState> Npcs { get; set; }
    public ConcurrentBag<PlayerConnection> Players { get; set; }

    public MapState(MapWithId data)
    {
        Id = data.Id;
        Data = data.Map;
    }

    public void Enter(PlayerConnection player)
    {
        Players.Add(player);
    }

    public void Leave(PlayerConnection player)
    {
        Players = new ConcurrentBag<PlayerConnection>(Players.Where(p => p != player));
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