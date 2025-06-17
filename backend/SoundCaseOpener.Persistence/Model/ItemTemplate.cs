using SoundCaseOpener.Shared;

namespace SoundCaseOpener.Persistence.Model;

public class ItemTemplate
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required Rarity Rarity { get; set; }
    public required List<Item> Items { get; set; }
    public required List<CaseItem> CaseTemplates { get; set; }
}
