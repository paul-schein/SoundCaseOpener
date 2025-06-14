using Microsoft.EntityFrameworkCore;
using SoundCaseOpener.Persistence.Model;

namespace SoundCaseOpener.Persistence.Repositories;

public interface IUserRepository
{
    public ValueTask<User?> GetUserByUserNameAsync(string username);
    public ValueTask<User?> GetUserByIdAsync(int id, bool tracking = false);
    public ValueTask<bool> CheckUserExistsAsync(string username);
    public ValueTask<Role?> GetUserRoleByIdAsync(int id);
    public void AddUser(User user);
}

internal sealed class UserRepository(DbSet<User> users) : IUserRepository
{
    private IQueryable<User> Users => users;
    private IQueryable<User> UsersNoTracking => users.AsNoTracking();
    
    public async ValueTask<User?> GetUserByUserNameAsync(string username) =>
        await UsersNoTracking
            .FirstOrDefaultAsync(u => u.Username == username);

    public async ValueTask<User?> GetUserByIdAsync(int id, bool tracking)
    {
        return await GetQueryableByTracking(tracking)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async ValueTask<bool> CheckUserExistsAsync(string username) => 
        await UsersNoTracking.AnyAsync(u => u.Username == username);

    public async ValueTask<Role?> GetUserRoleByIdAsync(int id) => 
        await UsersNoTracking
              .Where(u => u.Id == id)
              .Select(u => u.Role)
              .FirstOrDefaultAsync();

    public void AddUser(User user)
    {
        users.Add(user);
    }
    
    private IQueryable<User> GetQueryableByTracking(bool tracking) => 
        tracking ? Users : UsersNoTracking;
}
