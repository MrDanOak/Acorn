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
    public IEnumerable<Character> Characters { get; set; }
}


public record Character(
    string Accounts_Username,
    string Name,
    string Title,
    string Home,
    string Fiance,
    string Partner,
    int Admin,
    int Class,
    int Gender,
    int Race,
    int HairStyle,
    int HairColor,
    int Map,
    int X,
    int Y,
    int Direction,
    int Level,
    int Exp,
    int Hp,
    int Tp,
    int Str,
    int Wis,
    int Agi,
    int Con,
    int Cha,
    int StatPoints,
    int SkillPoints,
    int Karma,
    bool Sitting,
    bool Hidden,
    bool NoInteract,
    int BankMax,
    int GoldBank,
    int Usage,
    string Inventory,
    string Bank,
    string Paperdoll,
    string Spells,
    string Guild,
    int GuildRank,
    string GuildRankString,
    string Quest
);

public record Guild(
    string Tag,
    string Name,
    string Description,
    string Ranks,
    int Bank
);