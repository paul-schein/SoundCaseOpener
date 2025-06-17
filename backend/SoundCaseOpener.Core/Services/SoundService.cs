using OneOf;
using OneOf.Types;
using SoundCaseOpener.Persistence.Model;
using SoundCaseOpener.Persistence.Util;

namespace SoundCaseOpener.Core.Services;

public interface ISoundService
{
    public ValueTask<OneOf<Success<IReadOnlyCollection<Sound>>, NotFound>> GetAllSoundsOfUserAsync(int userId);
    public ValueTask<OneOf<Success<Sound>, NotFound>> ChangeSoundNameAsync(int id, string newName);
}

internal sealed class SoundService(IUnitOfWork uow,
                                   ILogger<SoundService> logger) : ISoundService
{
    public async ValueTask<OneOf<Success<IReadOnlyCollection<Sound>>, NotFound>> GetAllSoundsOfUserAsync(int userId)
    {
        if (!await uow.UserRepository.CheckUserExistsByIdAsync(userId))
        {
            logger.LogInformation("User with id {UserId} not found", userId);
            return new NotFound();
        }
        
        return new Success<IReadOnlyCollection<Sound>>(await uow.SoundRepository.GetAllItemsOfUserAsync(userId));
    }

    public async ValueTask<OneOf<Success<Sound>, NotFound>> ChangeSoundNameAsync(int id, string newName)
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
        
        return new Success<Sound>(sound);
    }
}
