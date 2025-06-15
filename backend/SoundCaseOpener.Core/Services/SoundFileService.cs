using Microsoft.Extensions.Options;
using OneOf;
using OneOf.Types;
using SoundCaseOpener.Core.Util;
using SoundCaseOpener.Persistence.Model;
using SoundCaseOpener.Persistence.Util;

namespace SoundCaseOpener.Core.Services;

public interface ISoundFileService
{
    public ValueTask<OneOf<SoundFile, NotFound>> GetSoundFileByIdAsync(int id);
    public ValueTask<IReadOnlyCollection<SoundFile>> GetAllSoundFilesAsync();
    public ValueTask<SoundFile> AddSoundFileAsync(
        string name,
        Func<Stream, CancellationToken, Task> fileCopier,
        string fileExtension,
        CancellationToken cancellationToken);
    public ValueTask<OneOf<Success, NotFound>> DeleteSoundFileAsync(int id);
    public ValueTask<OneOf<Success<SoundFile>, NotFound>> ChangeSoundFileNameAsync(int id, string newName);
}

internal sealed class SoundFileService(IUnitOfWork uow,
                                       ILogger<SoundFileService> logger,
                                       IOptions<Settings> settings) : ISoundFileService
{
    public async ValueTask<OneOf<SoundFile, NotFound>> GetSoundFileByIdAsync(int id)
    {
        SoundFile? soundFile = await uow.SoundFileRepository.GetByIdAsync(id);
        if (soundFile is null)
        {
            logger.LogWarning("Sound file with id {Id} not found", id);
            return new NotFound();
        }
        
        return soundFile;
    }

    public async ValueTask<IReadOnlyCollection<SoundFile>> GetAllSoundFilesAsync() => 
        await uow.SoundFileRepository.GetAllAsync();

    public async ValueTask<SoundFile> AddSoundFileAsync(string name,
                                                               Func<Stream, CancellationToken, Task> fileCopier,
                                                               string fileExtension,
                                                               CancellationToken cancellationToken)
    {
        string fileName = $"{Guid.NewGuid()}{fileExtension}";
        string filePath = Path.Combine(settings.Value.SoundFilesPath, fileName);
        
        if (!Directory.Exists(settings.Value.SoundFilesPath))
        {
            Directory.CreateDirectory(settings.Value.SoundFilesPath);
        }
        
        await using var stream = new FileStream(filePath, FileMode.Create);
        await fileCopier(stream, cancellationToken);
        
        SoundFile soundFile = new()
        {
            FilePath = fileName,
            Name = name,
            SoundTemplates = []
        };
        
        uow.SoundFileRepository.Add(soundFile);
        await uow.SaveChangesAsync();

        logger.LogInformation("Sound file with id {Id} and filepath {filepath} added", 
                              soundFile.Id, soundFile.FilePath);
        
        return soundFile;
    }

    public async ValueTask<OneOf<Success, NotFound>> DeleteSoundFileAsync(int id)
    {
        SoundFile? soundFile = await uow.SoundFileRepository.GetByIdAsync(id, true);
        if (soundFile is null)
        {
            logger.LogWarning("Sound file with id {Id} not found", id);
            return new NotFound();
        }
        
        uow.SoundFileRepository.Remove(soundFile);
        await uow.SaveChangesAsync();
        
        string wholeFilePath = Path.Combine(settings.Value.SoundFilesPath, soundFile.FilePath);
        File.Delete(wholeFilePath);
        
        logger.LogInformation("Sound file with id {Id} and filepath {FilePath} deleted", 
                              soundFile.Id, wholeFilePath);
        
        return new Success();
    }

    public async ValueTask<OneOf<Success<SoundFile>, NotFound>> ChangeSoundFileNameAsync(int id, string newName)
    {
        SoundFile? soundFile = await uow.SoundFileRepository.GetByIdAsync(id, true);
        if (soundFile is null)
        {
            logger.LogWarning("Sound file with id {Id} not found", id);
            return new NotFound();
        }
        
        soundFile.Name = newName;
        await uow.SaveChangesAsync();
        
        logger.LogInformation("Sound file with id {Id} changed name to {NewName}", 
                              soundFile.Id, newName);
        
        return new Success<SoundFile>(soundFile);
    }
}