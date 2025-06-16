using OneOf;
using OneOf.Types;
using SoundCaseOpener.Core.Logic;
using SoundCaseOpener.Persistence.Model;
using SoundCaseOpener.Persistence.Repositories;
using SoundCaseOpener.Persistence.Util;

namespace SoundCaseOpener.Core.Services;

public interface ICaseService
{
    public ValueTask<OneOf<Success<IReadOnlyCollection<Case>>, NotFound>> GetAllCasesOfUserAsync(int userId);
    public ValueTask<OneOf<Success<Case>, NotFound>> ChangeCaseNameAsync(int id, string newName);
    public ValueTask<OneOf<Success<Sound>, Empty, NotFound>> OpenCaseAsync(int caseId);

    public readonly record struct Empty;
}

internal sealed class CaseService(IUnitOfWork uow,
                                  ILogger<CaseService> logger) : ICaseService
{
    public async ValueTask<OneOf<Success<IReadOnlyCollection<Case>>, NotFound>> GetAllCasesOfUserAsync(int userId)
    {
        if (!await uow.UserRepository.CheckUserExistsByIdAsync(userId))
        {
            logger.LogInformation("User with id {UserId} not found", userId);
            return new NotFound();
        }

        return new Success<IReadOnlyCollection<Case>>(await uow.CaseRepository.GetAllItemsOfUserAsync(userId));
    }

    public async ValueTask<OneOf<Success<Case>, NotFound>> ChangeCaseNameAsync(int id, string newName)
    {
        Case? caseItem = await uow.CaseRepository.GetByIdAsync(id, true);
        if (caseItem is null)
        {
            logger.LogInformation("Case with id {Id} not found", id);
            return new NotFound();
        }
        
        caseItem.Name = newName;
        await uow.SaveChangesAsync();
        
        logger.LogInformation("Case with id {Id} changed name to {NewName}", id, newName);
        
        return new Success<Case>(caseItem);
    }

    public async ValueTask<OneOf<Success<Sound>, ICaseService.Empty, NotFound>> OpenCaseAsync(int caseId)
    {
        Case? caseItem = await uow.CaseRepository.GetByIdAsync(caseId, true);
        if (caseItem is null)
        {
            logger.LogInformation("Case with id {Id} not found", caseId);
            return new NotFound();
        }

        IReadOnlyCollection<ICaseItemRepository.CaseItemSoundTemplate> soundTemplates = 
            await uow.CaseItemRepository.GetSoundTemplatesInCaseTemplateAsync(caseItem.TemplateId, true);

        if (soundTemplates.Count == 0)
        {
            uow.CaseRepository.Remove(caseItem);
            await uow.SaveChangesAsync();
            
            logger.LogInformation("Case with id {CaseId} opened, but no sound templates found, case removed", caseId);
            
            return new ICaseService.Empty();
        }
        
        double weightSum = soundTemplates.Sum(st => st.Weight);
        double randomValue = Random.Shared.NextDouble() * weightSum;

        double currentWeight = 0;
        foreach (var soundTemplate in soundTemplates)
        {
            currentWeight += soundTemplate.Weight;
            if (randomValue <= currentWeight)
            {
                Sound sound = soundTemplate.Template.ToRandomSound(caseItem.Owner);
                
                uow.SoundRepository.Add(sound);
                uow.CaseRepository.Remove(caseItem);
                
                await uow.SaveChangesAsync();
                
                logger.LogInformation("Case with id {CaseId} opened, sound {SoundId} created from template {TemplateId}",
                    caseId, sound.Id, soundTemplate.Template.Id);
                
                return new Success<Sound>(sound);
            }
        }
        
        logger.LogError("No sound template found for case with id {CaseId}, this should never happen", caseId);
        return new ICaseService.Empty();
    }
}
