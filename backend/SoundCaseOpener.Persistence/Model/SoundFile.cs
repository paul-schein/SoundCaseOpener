namespace SoundCaseOpener.Persistence.Model;

public class SoundFile
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string FilePath { get; set; }
}
