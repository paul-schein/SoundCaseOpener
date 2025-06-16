using OneOf;
using OneOf.Types;
using SoundCaseOpener.Persistence.Model;
using SoundCaseOpener.Persistence.Util;

namespace SoundCaseOpener.Core.Services;

public interface ISoundService
{
    public ValueTask<IReadOnlyCollection<Sound>> GetAllSoundsOfUserAsync(int userId);
    public ValueTask<OneOf<Success, NotFound>> ChangeSoundName(int id, string newName);
    public ValueTask<OneOf<Success<Sound>, NotFound>> CreateSoundFromTemplateForUser(int userId, int soundTemplateId);
}

internal sealed class SoundService(IUnitOfWork uow,
                                   ILogger<SoundService> logger) : ISoundService
{
    public async ValueTask<IReadOnlyCollection<Sound>> GetAllSoundsOfUserAsync(int userId) => 
        await uow.SoundRepository.GetAllItemsOfUserAsync(userId);

    public async ValueTask<OneOf<Success, NotFound>> ChangeSoundName(int id, string newName)
    {
        Sound? sound = await uow.SoundRepository.GetByIdAsync(id, true);
        if (sound is null)
        {
            logger.LogInformation("Sound with id {Id} not found", id);
            return new NotFound();
        }
        
        sound.Name = newName;
        await uow.SaveChangesAsync();
        
        logger.LogInformation("Sound with id {Id} changed name to {NewName}", id, newName);
        
        return new Success();
    }

    public async ValueTask<OneOf<Success<Sound>, NotFound>> CreateSoundFromTemplateForUser(
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
        
        Sound sound = new()
        {
            Name = soundTemplate.Name,
            Owner = user,
            Template = soundTemplate,
            Cooldown = Random.Shared.Next(soundTemplate.MinCooldown, soundTemplate.MaxCooldown + 1),
            LastTimeUsed = null
        };
        
        uow.SoundRepository.Add(sound);
        await uow.SaveChangesAsync();
        
        logger.LogInformation("Sound with id {Id} created from template {TemplateId} for user {UserId}", 
                              sound.Id, soundTemplateId, userId);
        
        return new Success<Sound>(sound);
    }
}
