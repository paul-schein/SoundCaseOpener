using Microsoft.EntityFrameworkCore;
using SoundCaseOpener.Persistence.Model;

namespace SoundCaseOpener.Persistence.Repositories;

public interface ISoundTemplateRepository
{
    public ValueTask<IReadOnlyCollection<SoundTemplate>> GetAllAsync();
    public ValueTask<SoundTemplate?> GetByIdAsync(int id, bool tracking = false);
    public void Add(SoundTemplate soundTemplate);
    public void Remove(SoundTemplate soundTemplate);
}

internal sealed class SoundTemplateRepository(DbSet<SoundTemplate> soundTemplates) : ISoundTemplateRepository
{
    private IQueryable<SoundTemplate> SoundTemplates => soundTemplates;
    private IQueryable<SoundTemplate> SoundTemplatesNoTracking => soundTemplates.AsNoTracking();
    
    public async ValueTask<IReadOnlyCollection<SoundTemplate>> GetAllAsync() => 
        await SoundTemplatesNoTracking.ToListAsync();

    public async ValueTask<SoundTemplate?> GetByIdAsync(int id, bool tracking = false) => 
        await GetQueryableByTracking(tracking)
            .FirstOrDefaultAsync(st => st.Id == id);

    public void Add(SoundTemplate soundTemplate)
    {
        soundTemplates.Add(soundTemplate);
    }

    public void Remove(SoundTemplate soundTemplate)
    { 
        soundTemplates.Remove(soundTemplate);
    }
    
    private IQueryable<SoundTemplate> GetQueryableByTracking(bool tracking) => 
        tracking ? SoundTemplates : SoundTemplatesNoTracking;
}
