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
    public ValueTask<OneOf<Success, NotFound>> DeleteAsync(int id);
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

    public async ValueTask<OneOf<Success, NotFound>> DeleteAsync(int id)
    {
        CaseTemplate? caseTemplate = await uow.CaseTemplateRepository.GetByIdAsync(id);
        if (caseTemplate is null)
        {
            logger.LogWarning("Case template with id {Id} not found", id);
            return new NotFound();
        }
        
        uow.CaseTemplateRepository.Remove(caseTemplate);
        await uow.SaveChangesAsync();
        
        logger.LogInformation("Case template with id {Id} deleted", id);
        
        return new Success();
    }
}