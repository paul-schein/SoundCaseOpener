namespace SoundCaseOpener.Persistence.Model;

public class CaseItem
{
    public int CaseTemplateId { get; set; }
    public int ItemTemplateId { get; set; }
    public required double Chance { get; set; }
}
