namespace SoundCaseOpener.Persistence.Model;

public class CaseTemplate : ItemTemplate
{
    public required List<CaseItem> ItemTemplates { get; set; }
}