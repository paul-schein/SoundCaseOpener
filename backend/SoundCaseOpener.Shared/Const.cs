namespace SoundCaseOpener.Shared;

public static class Const
{
    public static readonly DateTimeZone TimeZone = DateTimeZoneProviders.Tzdb["Europe/Vienna"];
    public const int MaxUsernameLength = 30;
    public const int MaxSoundNameLength = 50;
    public const int MaxItemNameLength = 50;
    public const int MaxItemDescriptionLength = 200;
}
