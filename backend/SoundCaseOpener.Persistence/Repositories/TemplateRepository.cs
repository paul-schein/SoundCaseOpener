using Microsoft.EntityFrameworkCore;
using SoundCaseOpener.Persistence.Model;

namespace SoundCaseOpener.Persistence.Repositories;

public interface ITemplateRepository<T> where T : ItemTemplate
{
    public ValueTask<IReadOnlyCollection<T>> GetAllAsync();
    public ValueTask<T?> GetByIdAsync(int id, bool tracking = false);
    public void Add(T itemTemplate);
    public void Remove(T itemTemplate);
    public ValueTask<bool> CheckIfExists(int id);
}

internal sealed class TemplateRepository<T>(DbSet<T> templates) : ITemplateRepository<T> where T : ItemTemplate
{
    private IQueryable<T> Templates => templates;
    private IQueryable<T> TemplatesNoTracking => templates.AsNoTracking();

    public async ValueTask<IReadOnlyCollection<T>> GetAllAsync() => 
        await TemplatesNoTracking.ToListAsync();

    public async ValueTask<T?> GetByIdAsync(int id, bool tracking = false) => 
        await GetQueryableByTracking(tracking)
            .FirstOrDefaultAsync(t => t.Id == id);
    
    public void Add(T itemTemplate)
    {
        templates.Add(itemTemplate);
    }

    public void Remove(T itemTemplate)
    {
        templates.Remove(itemTemplate);
    }

    public async ValueTask<bool> CheckIfExists(int id) => 
        await TemplatesNoTracking.AnyAsync(t => t.Id == id);

    private IQueryable<T> GetQueryableByTracking(bool tracking) => 
        tracking ? Templates : TemplatesNoTracking;
}
