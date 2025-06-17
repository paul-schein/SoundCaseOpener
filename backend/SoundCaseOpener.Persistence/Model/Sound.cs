namespace SoundCaseOpener.Persistence.Model;

public class Sound : Item
{
    public required int Cooldown { get; set; }
    public Instant? LastTimeUsed { get; set; }
}