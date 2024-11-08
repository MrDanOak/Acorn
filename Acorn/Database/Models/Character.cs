﻿using Acorn.Data.Repository;
using Moffat.EndlessOnline.SDK.Protocol;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Moffat.EndlessOnline.SDK.Protocol.Pub;
using OneOf;
using OneOf.Types;
using System.Collections.Concurrent;

namespace Acorn.Data.Models;

public class Character
{
    public string Accounts_Username { get; set; }
    public string? Name { get; set; }
    public string? Title { get; set; }
    public string? Home { get; set; }
    public string? Fiance { get; set; }
    public string? Partner { get; set; }
    public AdminLevel Admin { get; set; }
    public int Class { get; set; }
    public Gender Gender { get; set; }
    public int Race { get; set; }
    public int HairStyle { get; set; }
    public int HairColor { get; set; }
    public int Map { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public Direction Direction { get; set; }
    public int Level { get; set; }
    public int Exp { get; set; }
    public int MaxHp { get; set; }
    public int Hp { get; set; }
    public int MaxTp { get; set; }
    public int Tp { get; set; }
    public int MaxSp { get; set; }
    public int Sp { get; set; }
    public int Str { get; set; }
    public int Wis { get; set; }
    public int Agi { get; set; }
    public int Con { get; set; }
    public int Cha { get; set; }
    public int MinDamage { get; set; }
    public int MaxDamage { get; set; }
    public int MaxWeight { get; set; }
    public int StatPoints { get; set; }
    public int SkillPoints { get; set; }
    public int Karma { get; set; }
    public SitState SitState { get; set; }
    public bool Hidden { get; set; }
    public bool NoInteract { get; set; }
    public int BankMax { get; set; }
    public int GoldBank { get; set; }
    public int Usage { get; set; }
    public Inventory Inventory { get; set; } = new([]);
    public Bank Bank { get; set; } = new([]);
    public Paperdoll Paperdoll { get; set; } = new();

    //TODO: Add spells
    //TODO: Add guilds
    //TODO: Add quests

    public void CalculateStats(Ecf classes)
    {
        classes.GetClass(Class).Switch(
            success =>
            {
                var @class = success.Value;

                MaxHp = 100;
                MaxTp = 100;
                MaxSp = 100;
                MinDamage = 100;
                MaxDamage = 150;
                MaxWeight = 100;

                Hp = Hp > MaxHp ? MaxHp : Hp;
                Str = @class.Str + Str;
                Wis = @class.Wis + Wis;
                Agi = @class.Agi + Agi;
                Con = @class.Con + Con;
                Cha = @class.Cha + Cha;
            },
            error =>
            {
                throw new Exception("Could not calculate stats for character " + Name + ". Error: " + error);
            }
        );
    }

    public OneOf<Success<IEnumerable<Item>>, Error<string>> Items()
    {
        return new Success<IEnumerable<Item>>(Inventory.Items.Select(x => new Item
        {
            Amount = x.Amount,
            Id = x.Id
        }));
    }

    public OneOf<Success<EquipmentPaperdoll>, Error<string>> Equipment()
    {
        return new Success<EquipmentPaperdoll>(new EquipmentPaperdoll()
        {
            Hat = Paperdoll.Hat,
            Necklace = Paperdoll.Necklace,
            Armor = Paperdoll.Armor,
            Belt = Paperdoll.Belt,
            Boots = Paperdoll.Boots,
            Gloves = Paperdoll.Gloves,
            Weapon = Paperdoll.Weapon,
            Shield = Paperdoll.Shield,
            Accessory = Paperdoll.Accessory,
            Ring = [Paperdoll.Ring1, Paperdoll.Ring2],
            Bracer = [Paperdoll.Bracer1, Paperdoll.Bracer2],
            Armlet = [Paperdoll.Armlet1, Paperdoll.Armlet2],
        });
    }

    public int Recover(int amount)
    {
        Hp = Hp switch
        {
            var hp when hp < MaxHp && hp + amount < MaxHp => hp + amount,
            _ => MaxHp
        };

        return Hp;
    }
}

public record Bank(ConcurrentBag<ItemWithAmount> Items);

public record Inventory(ConcurrentBag<ItemWithAmount> Items);