﻿using Acorn.Data.Models;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace Acorn.Extensions;
public static class CharacterExtensions
{
    public static CharacterSelectionListEntry AsCharacterListEntry(this Character c, int id)
        => new()
        {
            Admin = c.Admin,
            Equipment = new EquipmentCharacterSelect(),
            Gender = c.Gender,
            HairColor = c.HairColor,
            HairStyle = c.HairStyle,
            Level = c.Level,
            Name = c.Name,
            Skin = c.Race,
            Id = id
        };

    public static CharacterMapInfo AsCharacterMapInfo(this Character c, int playerId, WarpEffect wapEffect) => new()
    {
        ClassId = c.Class,
        Direction = c.Direction,
        Coords = new BigCoords { X = c.X, Y = c.Y },
        Equipment = new EquipmentMapInfo(),
        WarpEffect = wapEffect,
        Gender = c.Gender,
        GuildTag = "   ",//todo: guilds
        HairColor = c.HairColor,
        HairStyle = c.HairStyle,
        Hp = c.Hp,
        MaxHp = c.MaxHp,
        MapId = c.Map,
        MaxTp = c.MaxTp,
        Name = c.Name,
        Invisible = c.Hidden,
        Level = c.Level,
        PlayerId = playerId,
        SitState = c.SitState,
        Tp = c.Tp,
        Skin = c.Race
    };

    public static OnlinePlayer AsOnlinePlayer(this Character c) => new()
    {
        ClassId = c.Class,
        GuildTag = "   ", //todo: guilds
        Icon = (int)c.Admin switch
        {
            0 => CharacterIcon.Player,
            1 or 2 or 3 => CharacterIcon.Gm,
            _ => CharacterIcon.Hgm
            // todo: handle party
        },
        Level = c.Level,
        Name = c.Name,
        Title = c.Title ?? ""
    };

    public static CharacterStatsUpdate AsStatsUpdate(this Character c)
        => new()
        {
            MaxHp = c.MaxHp,
            MaxTp = c.MaxTp,
            MaxSp = c.MaxSp,
            BaseStats = c.AsBaseStats(),
            SecondaryStats = c.AsSecondaryStats(),
            MaxWeight = c.MaxWeight
        };

    public static CharacterBaseStats AsBaseStats(this Character c)
    => new()
    {
        Agi = c.Agi,
        Cha = c.Cha,
        Con = c.Con,
        Str = c.Str,
        Wis = c.Wis
    };

    public static CharacterSecondaryStats AsSecondaryStats(this Character c)
        => new()
        {
            Accuracy = 0,
            Armor = 0,
            Evade = 0,
            MinDamage = c.MinDamage,
            MaxDamage = c.MaxDamage
        };
}
