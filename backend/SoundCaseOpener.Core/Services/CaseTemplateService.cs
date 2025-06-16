using OneOf;
using OneOf.Types;
using SoundCaseOpener.Persistence.Model;
using SoundCaseOpener.Persistence.Util;

namespace SoundCaseOpener.Core.Services;

public interface ICaseTemplateService
{
    public ValueTask<IReadOnlyCollection<CaseTemplate>> GetAllAsync();
    public ValueTask<OneOf<CaseTemplate, NotFound>> GetByIdAsync(int id);
    public ValueTask<CaseTemplate> AddAsync(
        string name,
        string description,
        Rarity rarity);
    public ValueTask<OneOf<Success, Conflict, NotFound>> AddItemTemplateToCaseTemplateAsync(
        int caseTemplateId,
        int itemTemplateId,
        double weight);
    public ValueTask<OneOf<Success, NotFound>> RemoveItemTemplateFromCaseTemplateAsync(
        int caseTemplateId,
        int itemTemplateId);
    public ValueTask<OneOf<Success<IReadOnlyCollection<SoundTemplate>>, NotFound>> GetAllSoundTemplatesInCaseTemplateAsync(
        int caseTemplateId);
    public ValueTask<OneOf<Success, NotFound>> DeleteAsync(int id);
    
    public readonly record struct Conflict;
}

internal sealed class CaseTemplateService(IUnitOfWork uow,
                                          ILogger<CaseTemplateService> logger) : ICaseTemplateService
{
    public async ValueTask<IReadOnlyCollection<CaseTemplate>> GetAllAsync() => 
        await uow.CaseTemplateRepository.GetAllAsync();

    public async ValueTask<OneOf<CaseTemplate, NotFound>> GetByIdAsync(int id)
    {
        CaseTemplate? caseTemplate = await uow.CaseTemplateRepository.GetByIdAsync(id);
        if (caseTemplate is null)
        {
            logger.LogWarning("Case template with id {Id} not found", id);
            return new NotFound();
        }
        
        return caseTemplate;
    }

    public async ValueTask<CaseTemplate> AddAsync(
        string name, string description, Rarity rarity)
    {
        CaseTemplate caseTemplate = new()
        {
            Name = name,
            Description = description,
            Rarity = rarity,
            Items = [],
            ItemTemplates = [],
            CaseTemplates = []
        };
        uow.CaseTemplateRepository.Add(caseTemplate);
        await uow.SaveChangesAsync();
        
        logger.LogInformation("Case template with id {Id} added", caseTemplate.Id);
        
        return caseTemplate;
    }

    public async ValueTask<OneOf<Success, ICaseTemplateService.Conflict, NotFound>> AddItemTemplateToCaseTemplateAsync(
        int caseTemplateId, int itemTemplateId, double weight)
    {
        if (await uow.CaseItemRepository.CheckIfCaseItemExistsAsync(caseTemplateId, itemTemplateId))
        {
            return new ICaseTemplateService.Conflict();
        }
        
        CaseTemplate? caseTemplate = await uow.CaseTemplateRepository.GetByIdAsync(caseTemplateId, true);
        if (caseTemplate is null)
        {
            logger.LogWarning("Case template with id {Id} not found", caseTemplateId);
            return new NotFound();
        }
        
        ItemTemplate? itemTemplate = await uow.ItemTemplateRepository.GetByIdAsync(itemTemplateId, true);
        if (itemTemplate is null)
        {
            logger.LogWarning("Item template with id {Id} not found", itemTemplateId);
            return new NotFound();
        }
        
        CaseItem caseItem = new()
        {
            CaseTemplate = caseTemplate,
            ItemTemplate = itemTemplate,
            Weight = weight
        };
        uow.CaseItemRepository.Add(caseItem);
        await uow.SaveChangesAsync();
        
        logger.LogInformation("Item template with id {ItemTemplateId} added to case template with id {CaseTemplateId} with chance {Chance}",
                              itemTemplateId, caseTemplateId, weight);
        
        return new Success();
    }

    public async ValueTask<OneOf<Success, NotFound>> RemoveItemTemplateFromCaseTemplateAsync(
        int caseTemplateId, int itemTemplateId)
    {
        CaseItem? caseItem = await uow.CaseItemRepository.GetCaseItemByIdsAsync(caseTemplateId, itemTemplateId);
        if (caseItem is null)
        {
            logger.LogInformation("Case item with case template id {CaseTemplateId} and item template id {ItemTemplateId} not found",
                              caseTemplateId, itemTemplateId);
            return new NotFound();
        }
        
        uow.CaseItemRepository.Remove(caseItem);
        await uow.SaveChangesAsync();
        
        logger.LogInformation("Item template with id {ItemTemplateId} removed from case template with id {CaseTemplateId}",
                              itemTemplateId, caseTemplateId);
        
        return new Success();
    }

    public async ValueTask<OneOf<Success<IReadOnlyCollection<SoundTemplate>>, NotFound>> GetAllSoundTemplatesInCaseTemplateAsync(
        int caseTemplateId)
    {
        if (!await uow.CaseTemplateRepository.CheckIfExists(caseTemplateId))
        {
            return new NotFound();
        }
        
        return new Success<IReadOnlyCollection<SoundTemplate>>(
                                await uow.CaseItemRepository.GetSoundTemplatesInCaseTemplateAsync(caseTemplateId));
    }
    
    public async ValueTask<OneOf<Success, NotFound>> DeleteAsync(int id)
    {
        CaseTemplate? caseTemplate = await uow.CaseTemplateRepository.GetByIdAsync(id, true);
        if (caseTemplate is null)
        {
            logger.LogInformation("Case template with id {Id} not found", id);
            return new NotFound();
        }
        
        uow.CaseTemplateRepository.Remove(caseTemplate);
        await uow.SaveChangesAsync();
        
        logger.LogInformation("Case template with id {Id} deleted", id);
        
        return new Success();
    }
}