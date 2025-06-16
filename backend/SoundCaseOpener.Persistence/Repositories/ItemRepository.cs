using Microsoft.EntityFrameworkCore;
using SoundCaseOpener.Persistence.Model;

namespace SoundCaseOpener.Persistence.Repositories;

public interface IItemRepository<T> where T : Item
{
    public ValueTask<IReadOnlyCollection<T>> GetAllItemsOfUserAsync(int userId);
    public ValueTask<T?> GetByIdAsync(int id, bool tracking = false);
    public void Add(T item);
    public void Remove(T item);
}

internal sealed class ItemRepository<T>(DbSet<T> items) : IItemRepository<T> where T : Item
{
    private IQueryable<T> Items => items;
    private IQueryable<T> ItemsNoTracking => items.AsNoTracking();
    
    public async ValueTask<IReadOnlyCollection<T>> GetAllItemsOfUserAsync(int userId) =>
        await ItemsNoTracking
              .Where(i => i.OwnerId == userId)
              .ToListAsync();

    public async ValueTask<T?> GetByIdAsync(int id, bool tracking = false) => 
        await GetQueryableByTracking(tracking)
            .FirstOrDefaultAsync(i => i.Id == id);

    public void Add(T item)
    {
        items.Add(item);
    }

    public void Remove(T item)
    {
        items.Remove(item);
    }
    
    private IQueryable<T> GetQueryableByTracking(bool tracking) => 
        tracking ? Items : ItemsNoTracking;
}
