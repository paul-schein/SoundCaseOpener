namespace SoundCaseOpener.Persistence.Model;

public class SoundTemplate : ItemTemplate
{
    public required int MinCooldown { get; set; }
    public required int MaxCooldown { get; set; }
    public int SoundFileId { get; set; }
    public required SoundFile SoundFile { get; set; }
}
