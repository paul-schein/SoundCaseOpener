using OneOf;
using OneOf.Types;
using SoundCaseOpener.Persistence.Util;

namespace SoundCaseOpener.Core.Services;

public interface IItemTemplateService
{
    public ValueTask<OneOf<Success, NotFound>> DeleteItemTemplateAsync(int id);
}

public class ItemTemplateService(IUnitOfWork uow,
                                 ILogger<ItemTemplateService> logger) : IItemTemplateService
{
    public async ValueTask<OneOf<Success, NotFound>> DeleteItemTemplateAsync(int id)
    {
        var itemTemplate = await uow.ItemTemplateRepository.GetByIdAsync(id);
        if (itemTemplate is null)
        {
            logger.LogInformation("Item template with id {Id} not found", id);
            return new NotFound();
        }

        uow.ItemTemplateRepository.Remove(itemTemplate);
        await uow.SaveChangesAsync();
        
        logger.LogInformation("Item template with id {Id} deleted", id);
        
        return new Success();
    }
}
