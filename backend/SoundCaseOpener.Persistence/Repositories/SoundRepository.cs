using Microsoft.EntityFrameworkCore;
using SoundCaseOpener.Persistence.Model;

namespace SoundCaseOpener.Persistence.Repositories;

public interface ISoundRepository
{
    public ValueTask<IReadOnlyCollection<Sound>> GetAllItemsOfUserAsync(int userId);
    public ValueTask<Sound?> GetByIdAsync(int id, bool tracking = false);
    public void Add(Sound item);
    public void Remove(Sound item);
}

internal sealed class SoundRepository(DbSet<Sound> sounds) : ISoundRepository
{
    private IQueryable<Sound> Sounds => sounds;
    private IQueryable<Sound> SoundsNoTracking => sounds.AsNoTracking();

    public async ValueTask<IReadOnlyCollection<Sound>> GetAllItemsOfUserAsync(int userId) =>
        await SoundsNoTracking
            .Where(s => s.OwnerId == userId)
            .Include(s => s.Owner)
            .Include(s => s.Template)
            .ThenInclude(st => ((SoundTemplate)st).SoundFile)
            .ToListAsync();

    public async ValueTask<Sound?> GetByIdAsync(int id, bool tracking) =>
        await GetQueryableByTracking(tracking)
              .Include(s => s.Owner)
              .Include(s => s.Template)
              .ThenInclude(st => ((SoundTemplate)st).SoundFile)
              .FirstOrDefaultAsync(s => s.Id == id);

    public void Add(Sound item)
    {
        sounds.Add(item);
    }

    public void Remove(Sound item)
    {
        sounds.Remove(item);
    }

    private IQueryable<Sound> GetQueryableByTracking(bool tracking) =>
        tracking ? Sounds : SoundsNoTracking;
}
