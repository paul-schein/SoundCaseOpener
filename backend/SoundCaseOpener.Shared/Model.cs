namespace SoundCaseOpener.Shared;

public enum Role
{
    User = 0,
    Admin = 10
}

public enum Rarity
{
    Common = 0,
    Uncommon = 5,
    Rare = 10,
    Epic = 15,
    Legendary = 20
}

public sealed record Lobby(string Id, string Name, int UserCount);