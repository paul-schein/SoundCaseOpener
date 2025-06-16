using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using OneOf;
using OneOf.Types;
using SoundCaseOpener.Persistence.Model;
using SoundCaseOpener.Persistence.Util;
using SoundCaseOpener.Shared;

namespace SoundCaseOpener.Core.Services;

public interface ICaseService
{
    public ValueTask<IReadOnlyCollection<Case>> GetAllCasesOfUserAsync(int userId);
    public ValueTask<OneOf<Success, NotFound>> ChangeCaseNameAsync(int id, string newName);
    public ValueTask<OneOf<Success<Case>, NotFound>> CreateCaseFromTemplateForUserAsync(int userId, int soundTemplateId);
    public ValueTask<OneOf<Success<Sound>, Empty, NotFound>> OpenCaseAsync(int caseId);

    public readonly record struct Empty;
}

internal sealed class CaseService(IUnitOfWork uow,
                                 ILogger<CaseService> logger) : ICaseService
{
    public async ValueTask<IReadOnlyCollection<Case>> GetAllCasesOfUserAsync(int userId) =>
        await uow.CaseRepository.GetAllItemsOfUserAsync(userId);

    public async ValueTask<OneOf<Success, NotFound>> ChangeCaseNameAsync(int id, string newName)
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
        
        return new Success();
    }

    public async ValueTask<OneOf<Success<Case>, NotFound>> CreateCaseFromTemplateForUserAsync(
        int userId, int soundTemplateId)
    {
        User? user = await uow.UserRepository.GetUserByIdAsync(userId, true);
        if (user is null)
        {
            logger.LogInformation("User with id {UserId} not found", userId);
            return new NotFound();
        }
        
        SoundTemplate? soundTemplate = await uow.SoundTemplateRepository.GetByIdAsync(soundTemplateId, true);
        if (soundTemplate is null)
        {
            logger.LogInformation("Sound template with id {SoundTemplateId} not found", soundTemplateId);
            return new NotFound();
        }
        
        Case caseItem = new()
        {
            Name = soundTemplate.Name,
            Owner = user,
            Template = soundTemplate
        };
        
        uow.CaseRepository.Add(caseItem);
        await uow.SaveChangesAsync();
        
        logger.LogInformation("Case created for user {UserId} from template {SoundTemplateId}", userId, soundTemplateId);
        
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

        IReadOnlyCollection<SoundTemplate> soundTemplates = 
            await uow.CaseItemRepository.GetSoundTemplatesInCaseTemplateAsync(caseItem.TemplateId);

        if (soundTemplates.Count == 0)
        {
            return new ICaseService.Empty();
        }
        
        int weightSum = soundTemplates.Sum(st => st);
    }
}
