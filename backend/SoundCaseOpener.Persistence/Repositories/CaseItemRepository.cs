using Microsoft.EntityFrameworkCore;
using SoundCaseOpener.Persistence.Model;

namespace SoundCaseOpener.Persistence.Repositories;

public interface ICaseItemRepository
{
    public void Add(CaseItem caseItem);
    public void Remove(CaseItem caseItem);
    public ValueTask<CaseItem?> GetCaseItemByIdsAsync(int caseTemplateId, 
                                                      int itemTemplateId, 
                                                      bool tracking = false);
    public ValueTask<IReadOnlyCollection<CaseItemSoundTemplate>> GetSoundTemplatesInCaseTemplateAsync(int caseTemplateId);
    public ValueTask<bool> CheckIfCaseItemExistsAsync(int caseTemplateId, int itemTemplateId);
    
    public readonly record struct CaseItemSoundTemplate(SoundTemplate Template, double Weight);
}

internal sealed class CaseItemRepository(DbSet<CaseItem> caseItems) : ICaseItemRepository
{
    private IQueryable<CaseItem> CaseItems => caseItems;
    private IQueryable<CaseItem> CaseItemsNoTracking => caseItems.AsNoTracking();

    private IQueryable<CaseItem> GetQueryableByTracking(bool tracking) => 
        tracking ? CaseItems : CaseItemsNoTracking;
    
    public async ValueTask<CaseItem?> GetCaseItemByIdsAsync(
        int caseTemplateId, 
        int itemTemplateId, 
        bool tracking = false) =>
    await GetQueryableByTracking(tracking)
        .Where(ci => ci.CaseTemplateId == caseTemplateId 
                     && ci.ItemTemplateId == itemTemplateId)
        .FirstOrDefaultAsync();

    public async ValueTask<IReadOnlyCollection<ICaseItemRepository.CaseItemSoundTemplate>> GetSoundTemplatesInCaseTemplateAsync(
        int caseTemplateId) =>
        (await CaseItemsNoTracking
            .Where(ci => ci.CaseTemplateId == caseTemplateId
                   && ci.ItemTemplate is SoundTemplate)
            .Select(ci => new
            {
                Template = ci.ItemTemplate,
                Weight = ci.Weight
            })
            .ToListAsync())
            .Select(x => 
                        new ICaseItemRepository.CaseItemSoundTemplate((SoundTemplate)x.Template, 
                                                                      x.Weight))
        .ToList();

    public async ValueTask<bool> CheckIfCaseItemExistsAsync(int caseTemplateId, int itemTemplateId) => 
        await CaseItemsNoTracking
            .AnyAsync(ci => ci.CaseTemplateId == caseTemplateId 
                            && ci.ItemTemplateId == itemTemplateId);

    public void Add(CaseItem caseItem)
    {
        caseItems.Add(caseItem);
    }

    public void Remove(CaseItem caseItem)
    {
        caseItems.Remove(caseItem);
    }
}
