namespace Acorn.Data.Models;

public class Account
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string Salt { get; set; }
    public string FullName { get; set; }
    public string Location { get; set; }
    public string Email { get; set; }
    public string Country { get; set; }
    public DateTime Created { get; set; }
    public DateTime LastUsed { get; set; }
    public IList<Character> Characters { get; set; }
}


public class Character
{
    public string Accounts_Username { get; set; }
    public string Name { get; set; }
    public string Title { get; set; }
    public string Home {  get; set; }
    public string Fiance { get; set; }
    public string Partner { get; set; }
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
    public string Inventory { get; set; }
    public string Bank { get; set; }
    public string Paperdoll { get; set; }
    public string Spells { get; set; }
    public string Guild { get; set; }
    public int GuildRank { get; set; }
    public string GuildRankString { get; set; }
    public string Quest { get; set; }

}

public class Guild
{
    public string Tag { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Ranks { get; set; }
    public int Bank { get; set; }
}