using Microsoft.EntityFrameworkCore;
using SoundCaseOpener.Persistence.Model;

namespace SoundCaseOpener.Persistence.Repositories;

public interface ISoundFileRepository
{
    public ValueTask<IReadOnlyCollection<SoundFile>> GetAllAsync();
    public ValueTask<SoundFile?> GetByIdAsync(int id, bool tracking);
    public void Add(SoundFile soundFile);
}

internal sealed class SoundFileRepository(DbSet<SoundFile> soundFiles) : ISoundFileRepository
{
    private IQueryable<SoundFile> SoundFiles => soundFiles;
    private IQueryable<SoundFile> SoundFilesNoTracking => soundFiles.AsNoTracking();
    
    public async ValueTask<IReadOnlyCollection<SoundFile>> GetAllAsync() =>
        await SoundFilesNoTracking.ToListAsync();

    public async ValueTask<SoundFile?> GetByIdAsync(int id, bool tracking) => 
        await GetQueryableByTracking(tracking)
            .FirstOrDefaultAsync(u => u.Id == id);

    public void Add(SoundFile soundFile)
    {
        soundFiles.Add(soundFile);
    }
    
    private IQueryable<SoundFile> GetQueryableByTracking(bool tracking) => 
        tracking ? SoundFiles : SoundFilesNoTracking;
}
