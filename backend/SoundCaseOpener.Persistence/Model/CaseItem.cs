namespace SoundCaseOpener.Persistence.Model;

public class CaseItem
{
    public int CaseTemplateId { get; set; }
    public int ItemTemplateId { get; set; }
    public required CaseTemplate CaseTemplate { get; set; }
    public required ItemTemplate ItemTemplate { get; set; }
    public required double Weight { get; set; }
}
