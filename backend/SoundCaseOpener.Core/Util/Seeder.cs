using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SoundCaseOpener.Persistence.Model;
using SoundCaseOpener.Persistence.Util;
using SoundCaseOpener.Shared;

namespace SoundCaseOpener.Core.Util;

public static class Seeder
{
    public static async ValueTask<bool> ImportSeedDataAsync(DatabaseContext context, Settings settings)
    {
        await context.Database.BeginTransactionAsync();
        
        IReadOnlyCollection<string> existingAdmins = context.Users
            .AsNoTracking()
            .Where(u => u.Role == Role.Admin)
            .Select(u => u.Username)
            .ToList();

        bool changesMade = false;
        foreach (string admin in settings.AdminUsers)
        {
            if (!existingAdmins.Contains(admin))
            {
                context.Users.Add(new User
                {
                    Username = admin,
                    Role = Role.Admin,
                    Items = []
                });
                changesMade = true;
            }
        }
        await context.SaveChangesAsync();
        
        if (changesMade)
        {
            await context.Database.CommitTransactionAsync();
        }
        else
        {
            await context.Database.RollbackTransactionAsync();
        }
        
        return changesMade;
    }
}
