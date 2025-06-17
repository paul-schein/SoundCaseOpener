using Microsoft.Extensions.Options;
using OneOf;
using OneOf.Types;
using SoundCaseOpener.Core.Logic;
using SoundCaseOpener.Core.Util;
using SoundCaseOpener.Persistence.Model;
using SoundCaseOpener.Persistence.Util;
using SoundCaseOpener.Shared;
using User = SoundCaseOpener.Persistence.Model.User;

namespace SoundCaseOpener.Core.Services;

public interface IUserService
{
    public ValueTask<OneOf<User, NotFound>> GetUserByUsername(string username);
    public ValueTask<OneOf<User, NotFound>> GetUserById(int id);
    public ValueTask<OneOf<Success<User>, Conflict>> AddUser(string username);
    public ValueTask<OneOf<Success, NotFound>> DeleteUser(int id);
    public ValueTask<OneOf<Role, NotFound>> GetUserRoleById(int id);

    public readonly record struct Conflict;
}

internal sealed class UserService(IUnitOfWork uow, 
                         ILogger<UserService> logger,
                         IOptions<Settings> settings) : IUserService
{
    public async ValueTask<OneOf<User, NotFound>> GetUserByUsername(string username)
    {
        User? user = await uow.UserRepository.GetUserByUserNameAsync(username);
        if (user is null)
        {
            logger.LogInformation("User with username {Username} not found", username);
            return new NotFound();
        }

        return user;
    }

    public async ValueTask<OneOf<User, NotFound>> GetUserById(int id)
    {
        User? user = await uow.UserRepository.GetUserByIdAsync(id, false);
        if (user is null)
        {
            logger.LogInformation("User with id {Id} not found", id);
            return new NotFound();
        }

        return user;
    }

    public async ValueTask<OneOf<Success<User>, IUserService.Conflict>> AddUser(string username)
    {
        if (await uow.UserRepository.CheckUserExistsByUsernameAsync(username))
        {
            logger.LogWarning("User with username {Username} already exists", username);
            return new IUserService.Conflict();
        }
        
        User user = new()
        {
            Username = username,
            Role = Role.User,
            Items = []
        };
        
        uow.UserRepository.AddUser(user);
        await CreateInitialCasesForUser(user);
        
        await uow.SaveChangesAsync();
        
        logger.LogInformation("User with username {Username} added", username);
        
        return new Success<User>(user);
    }

    public async ValueTask<OneOf<Success, NotFound>> DeleteUser(int id)
    {
        User? user = await uow.UserRepository.GetUserByIdAsync(id, true);
        if (user is null)
        {
            logger.LogInformation("User with id {Id} not found", id);
            return new NotFound();
        }
        
        uow.UserRepository.RemoveUser(user);
        await uow.SaveChangesAsync();
        
        logger.LogInformation("User with id {Id} deleted", id);
        
        return new Success();
    }

    public async ValueTask<OneOf<Role, NotFound>> GetUserRoleById(int id)
    {
        Role? role = await uow.UserRepository.GetUserRoleByIdAsync(id);
        if (role is null)
        {
            logger.LogInformation("Role for user with id {Id} not found", id);
            return new NotFound();
        }

        return role.Value;
    }
    
    private async ValueTask CreateInitialCasesForUser(User user)
    {
        List<CaseTemplate> templates = (await uow.CaseTemplateRepository.GetAllAsync(true))
            .ToList();
        
        if (templates.Count == 0)
        {
            logger.LogWarning("No case templates found, returning empty collection");
            return;
        }
        
        for (int i = 0; i < settings.Value.StarterCasesAmount; i++)
        {
            CaseTemplate template = templates.GetRandomElement();
            uow.CaseRepository.Add(template.ToCase(user));
        }
        
        logger.LogInformation("Created {Amount} initial cases for user {Username}", 
            settings.Value.StarterCasesAmount, user.Username);
    }
}
