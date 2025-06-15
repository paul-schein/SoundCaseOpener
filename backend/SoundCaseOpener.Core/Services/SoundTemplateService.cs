using OneOf;
using OneOf.Types;
using SoundCaseOpener.Persistence.Model;
using SoundCaseOpener.Persistence.Util;

namespace SoundCaseOpener.Core.Services;

public interface ISoundTemplateService
{
    public ValueTask<IReadOnlyCollection<SoundTemplate>> GetAllAsync();
    public ValueTask<OneOf<SoundTemplate, NotFound>> GetByIdAsync(int id);
    public ValueTask<OneOf<Success<SoundTemplate>, NotFound>> AddAsync(
        string name,
        string description,
        Rarity rarity,
        int minCooldown,
        int maxCooldown,
        int soundFileId);
    public ValueTask<OneOf<Success, NotFound>> DeleteAsync(int id);
}

internal sealed class SoundTemplateService(IUnitOfWork uow,
                                           ILogger<SoundTemplateService> logger) : ISoundTemplateService
{
    public async ValueTask<IReadOnlyCollection<SoundTemplate>> GetAllAsync() => 
        await uow.SoundTemplateRepository.GetAllAsync();

    public async ValueTask<OneOf<SoundTemplate, NotFound>> GetByIdAsync(int id)
    {
        SoundTemplate? soundTemplate = await uow.SoundTemplateRepository.GetByIdAsync(id);
        if (soundTemplate is null)
        {
            logger.LogWarning("Sound template with id {Id} not found", id);
            return new NotFound();
        }
        
        return soundTemplate;
    }

    public async ValueTask<OneOf<Success<SoundTemplate>, NotFound>> AddAsync(string name, 
                                                   string description, 
                                                   Rarity rarity, 
                                                   int minCooldown,
                                                   int maxCooldown, 
                                                   int soundFileId)
    {
        SoundFile? soundFile = await uow.SoundFileRepository.GetByIdAsync(soundFileId, true);
        if (soundFile is null)
        {
            logger.LogWarning("Sound file with id {Id} not found", soundFileId);
            return new NotFound();
        }
        
        SoundTemplate soundTemplate = new()
        {
            Name = name,
            Description = description,
            Rarity = rarity,
            MinCooldown = minCooldown,
            MaxCooldown = maxCooldown,
            SoundFile = soundFile,
            Items = [],
            CaseTemplates = []
        };
        
        uow.SoundTemplateRepository.Add(soundTemplate);
        await uow.SaveChangesAsync();
        
        logger.LogInformation("Sound template with id {Id} added", soundTemplate.Id);
        
        return new Success<SoundTemplate>(soundTemplate);
    }

    public async ValueTask<OneOf<Success, NotFound>> DeleteAsync(int id)
    {
        SoundTemplate? soundTemplate = await uow.SoundTemplateRepository.GetByIdAsync(id, true);
        if (soundTemplate is null)
        {
            logger.LogWarning("Sound template with id {Id} not found", id);
            return new NotFound();
        }
        
        uow.SoundTemplateRepository.Remove(soundTemplate);
        await uow.SaveChangesAsync();
        
        logger.LogInformation("Sound template with id {Id} deleted", id);
        
        return new Success();
    }
}
