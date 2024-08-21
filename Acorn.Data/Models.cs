using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using OneOf;
using OneOf.Types;

namespace Acorn.Data.Models;

public class Account
{
    public string Username { get; set; }
    public string? Password { get; set; }
    public string? Salt { get; set; }
    public string? FullName { get; set; }
    public string? Location { get; set; }
    public string? Email { get; set; }
    public string? Country { get; set; }
    public DateTime Created { get; set; }
    public DateTime LastUsed { get; set; }
    public IList<Character> Characters { get; set; }
}


public class Character
{
    public string Accounts_Username { get; set; }
    public string? Name { get; set; }
    public string? Title { get; set; }
    public string? Home {  get; set; }
    public string? Fiance { get; set; }
    public string? Partner { get; set; }
    public int Admin { get; set; }
    public int Class { get; set; }
    public int Gender { get; set; }
    public int Race { get; set; }
    public int HairStyle { get; set; }
    public int HairColor { get; set; }
    public int Map { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Direction { get; set; }
    public int Level { get; set; }
    public int Exp { get; set; }
    public int Hp { get; set; }
    public int Tp { get; set; }
    public int Str { get; set; }
    public int Wis { get; set; }
    public int Agi { get; set; }
    public int Con { get; set; }
    public int Cha { get; set; }
    public int StatPoints { get; set; }
    public int SkillPoints { get; set; }
    public int Karma { get; set; }
    public bool Sitting { get; set; }
    public bool Hidden { get; set; }
    public bool NoInteract { get; set; }
    public int BankMax { get; set; }
    public int GoldBank { get; set; }
    public int Usage { get; set; }
    public string? Inventory { get; set; }
    public string? Bank { get; set; }
    public string? Paperdoll { get; set; }
    public string? Spells { get; set; }
    public string? Guild { get; set; }
    public int GuildRank { get; set; }
    public string? GuildRankString { get; set; }
    public string? Quest { get; set; }

    public OneOf<Success<EquipmentPaperdoll>, Error<string>> Equipment()
    {
        if (string.IsNullOrWhiteSpace(Paperdoll))
        {
            var paperdoll = new EquipmentPaperdoll
            {
                Ring = [0, 0],
                Bracer = [0, 0],
                Armlet = [0, 0]
            };
            return new Success<EquipmentPaperdoll>(paperdoll);
        }

        var parts = Paperdoll.Split(",");
        if (parts.Length < 15)
        {
            return new Error<string>("Invalid paperdoll string length. Expected 15 parts, got " + parts.Length + ".");
        }

        return new Success<EquipmentPaperdoll>(new EquipmentPaperdoll()
        {
            Hat = int.Parse(parts[0]),
            Necklace = int.Parse(parts[1]),
            Armor = int.Parse(parts[2]),
            Belt = int.Parse(parts[3]),
            Boots = int.Parse(parts[4]),
            Gloves = int.Parse(parts[5]),
            Weapon = int.Parse(parts[6]),
            Shield = int.Parse(parts[7]),
            Accessory = int.Parse(parts[8]),
            Ring = [int.Parse(parts[9]), int.Parse(parts[10])],
            Bracer = [int.Parse(parts[11]), int.Parse(parts[12])],
            Armlet = [int.Parse(parts[13]), int.Parse(parts[14])],
        });
    }
}

public class Guild
{
    public string? Tag { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Ranks { get; set; }
    public int Bank { get; set; }
}