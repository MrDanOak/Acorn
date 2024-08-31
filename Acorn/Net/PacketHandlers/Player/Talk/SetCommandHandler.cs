﻿
using Moffat.EndlessOnline.SDK.Protocol;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace Acorn.Net.PacketHandlers.Player.Talk;
public class SetCommandHandler : ITalkHandler
{
    private readonly WorldState _world;
    const string Usage = "Usage: $set <player> <attribute> <value>";

    public SetCommandHandler(WorldState world)
    {
        _world = world;
    }

    public bool CanHandle(string command) 
        => string.Equals("set", command, StringComparison.InvariantCultureIgnoreCase);

    public async Task HandleAsync(PlayerConnection playerConnection, string command, params string[] args)
    {
        var author = playerConnection.Character;
        var map = _world.MapFor(playerConnection);

        if (args.Length < 3)
        {
            await playerConnection.Send(new TalkServerServerPacket 
            { 
                Message = Usage
            });
            return;
        }

        var target = _world.Players.FirstOrDefault(x => string.Equals(x.Character.Name, args[0], StringComparison.CurrentCultureIgnoreCase));
        if (target == null)
        {
            await playerConnection.Send(new TalkServerServerPacket
            {
                Message = $"Player {args[0]} not found."
            });
            return;
        }

        if (!int.TryParse(args[2], out var value))
        {
            await playerConnection.Send(new TalkServerServerPacket
            {
                Message = $"Value must be an integer. {Usage}"
            });
            return;
        }

        Action adjustment = args[1].ToLower() switch
        {
            "admin" => () => target.Character.Admin = (AdminLevel)value,
            "class" => () => target.Character.Class = value,
            "gender" => () => target.Character.Gender = (Gender)value,
            "level" => () => target.Character.Level = value,
            "skin" => () => target.Character.Race = value,
            "exp" => () => target.Character.Exp = value,
            "maxhp" => () => target.Character.MaxHp = value,
            "hp" => () => target.Character.Hp = value,
            "maxtp" => () => target.Character.MaxTp = value,
            "tp" => () => target.Character.Tp = value,
            "maxsp" => () => target.Character.MaxSp = value,
            "sp" => () => target.Character.Sp = value,
            "str" => () => target.Character.Str = value,
            "wis" => () => target.Character.Wis = value,
            "agi" => () => target.Character.Agi = value,
            "con" => () => target.Character.Con = value,
            "cha" => () => target.Character.Cha = value,
            "statpoints" => () => target.Character.StatPoints = value,
            "skillpoints" => () => target.Character.SkillPoints = value,
            "karma" => () => target.Character.Karma = value,
            "sitstate" => () => target.Character.SitState = (SitState)Enum.Parse(typeof(SitState), args[3], true),
            "hidden" => () => target.Character.Hidden = bool.Parse(args[3]),
            "nointeract" => () => target.Character.NoInteract = bool.Parse(args[3]),
            "bankmax" => () => target.Character.BankMax = value,
            "goldbank" => () => target.Character.GoldBank = value,
            "usage" => () => target.Character.Usage = value,
            _ => async () => await playerConnection.Send(new TalkServerServerPacket
            {
                Message = $"Attribute {args[2]} is not supported."
            })
        };

        adjustment();
        await playerConnection.Send(new TalkServerServerPacket
        {
            Message = $"Player {args[0]} had {args[1]} updated to {value}."
        });

        await _world.Refresh(playerConnection);
    }
}
