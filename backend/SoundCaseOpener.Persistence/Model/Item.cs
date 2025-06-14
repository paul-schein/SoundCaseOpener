namespace SoundCaseOpener.Persistence.Model;

public class Item
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int OwnerId { get; set; }
    public required User Owner { get; set; }
    public int TemplateId { get; set; }
    public required ItemTemplate Template { get; set; }
}