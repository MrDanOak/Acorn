using Acorn.Data.Repository;
using Acorn.Net;
using Moffat.EndlessOnline.SDK.Protocol.Map;
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

    public async Task Tick()
    {
        foreach (var map in Maps)
        {
            map.Tick();
        }
    }

    public MapState MapFor(PlayerConnection player) 
        => Maps.Single(x => x.HasPlayer(player));
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

    public void Enter(PlayerConnection player)
    {
        Players.Add(player);
        Players.Where(x => x != player)
            .ToList()
            .ForEach(async otherPlayer =>
                await otherPlayer.Send(new PlayersAgreeServerPacket
                {
                    Nearby = new NearbyInfo
                    {
                        Characters = Players.Where(x => x != otherPlayer).Select(x => x.Character.AsCharacterMapInfo(x.SessionId)).ToList(),
                        Items = [],
                        Npcs = []
                    }
                })
            );
    }

    public void Leave(PlayerConnection player)
    {
        Players = new ConcurrentBag<PlayerConnection>(Players.Where(p => p != player));
        Players.ToList().ForEach(async otherPlayer =>
        {
            await otherPlayer.Send(new PlayersRemoveServerPacket
            {
                PlayerId = player.SessionId,
            });

            await otherPlayer.Send(new AvatarRemoveServerPacket
            {
                PlayerId = player.SessionId,
                WarpEffect = WarpEffect.Admin
            });
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