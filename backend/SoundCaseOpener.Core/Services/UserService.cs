using System.Data;
using OneOf;
using OneOf.Types;
using SoundCaseOpener.Persistence.Model;
using SoundCaseOpener.Persistence.Util;

namespace SoundCaseOpener.Core.Services;

public interface IUserService
{
    public ValueTask<OneOf<User, NotFound>> GetUserByUsername(string username);
    public ValueTask<OneOf<User, NotFound>> GetUserById(int id);
    public ValueTask<OneOf<Success<User>, Conflict>> AddUser(string username);

    public readonly record struct Conflict;
}

internal sealed class UserService(IUnitOfWork uow, 
                         ILogger<UserService> logger) : IUserService
{
    public async ValueTask<OneOf<User, NotFound>> GetUserByUsername(string username)
    {
        User? user = await uow.UserRepository.GetUserByUserNameAsync(username);
        if (user is null)
        {
            logger.LogWarning("User with username {Username} not found", username);
            return new NotFound();
        }

        return user;
    }

    public async ValueTask<OneOf<User, NotFound>> GetUserById(int id)
    {
        User? user = await uow.UserRepository.GetUserByIdAsync(id, false);
        if (user is null)
        {
            logger.LogWarning("User with id {Id} not found", id);
            return new NotFound();
        }

        return user;
    }

    public async ValueTask<OneOf<Success<User>, IUserService.Conflict>> AddUser(string username)
    {
        if (await uow.UserRepository.CheckUserExistsAsync(username))
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
        logger.LogInformation("User with username {Username} added", username);
        
        return new Success<User>(user);
    }
}
