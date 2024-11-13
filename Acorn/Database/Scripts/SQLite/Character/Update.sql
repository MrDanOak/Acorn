UPDATE Characters
SET
    Title = @Title,
    Home = @Home,
    Fiance = @Fiance,
    Partner = @Partner,
    Admin = @Admin,
    Class = @Class,
    Gender = @Gender,
    Race = @Race,
    HairStyle = @HairStyle,
    HairColor = @HairColor,
    Map = @Map,
    X = @X,
    Y = @Y,
    Direction = @Direction,
    Level = @Level,
    Exp = @Exp,
    Hp = @Hp,
    Tp = @Tp,
    Str = @Str,
    Wis = @Wis,
    Agi = @Agi,
    Con = @Con,
    Cha = @Cha,
    StatPoints = @StatPoints,
    SkillPoints = @SkillPoints,
    Karma = @Karma,
    SitState = @SitState,
    Hidden = @Hidden,
    NoInteract = @NoInteract,
    BankMax = @BankMax,
    GoldBank = @GoldBank,
    Usage = @Usage
WHERE Name = @Name