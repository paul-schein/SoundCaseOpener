namespace SoundCaseOpener.Core.Util;

public sealed class Settings
{
    public const string SectionKey = "General";
    public required int StarterCasesAmount { get; init; }
    public required string SoundFilesPath { get; init; }
    public required IReadOnlyCollection<string> AllowedFileExtensions { get; init; }
    public required IReadOnlyCollection<string> AllowedFileTypes { get; init; }
    public required string ClientOrigin { get; init; }
}
