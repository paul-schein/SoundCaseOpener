using Microsoft.Extensions.DependencyInjection;
using Nito.AsyncEx;
using OneOf;
using OneOf.Types;
using SoundCaseOpener.Persistence.Model;
using SoundCaseOpener.Persistence.Util;
using SoundCaseOpener.Shared;

namespace SoundCaseOpener.Core.Services;

public interface ILobbyService
{
    public ValueTask<Lobby> CreateLobbyAsync(string connectionId, string name, int userId);
    public ValueTask<OneOf<Success<(IReadOnlyCollection<string> connections, string username)>, NotFound>> 
        JoinLobbyAsync(string connectionId, string lobbyId, int userId);
    public ValueTask<OneOf<Success<(IReadOnlyCollection<string> connections, string username)>, NotFound>> 
        LeaveLobbyAsync(string connectionId);
    public ValueTask<IReadOnlyCollection<Lobby>> GetLobbiesAsync();
    public ValueTask<IReadOnlyCollection<string>> GetUsersInLobbyAsync(string lobbyId);
    public ValueTask<OneOf<Success<(IReadOnlyCollection<string> connections, 
            string username, string filePath)>, NotFound, NotAllowed>> 
        PlaySoundAsync(int soundId);
    
    public readonly record struct NotAllowed;
}

public class LobbyService(IServiceScopeFactory scopeFactory, 
                          IClock clock,
                          ILogger<LobbyService> logger) : ILobbyService
{
    private readonly Dictionary<string, Lobby> _lobbies = [];
    private readonly Dictionary<string, HashSet<string>> _lobbyUsers = [];
    private readonly Dictionary<int, string> _userLobbies = [];
    private readonly Dictionary<string, string> _connections = [];
    private readonly Dictionary<string, int> _connectionUsers = [];
    private readonly AsyncReaderWriterLock _lobbiesLock = new();

    public async ValueTask<Lobby> CreateLobbyAsync(string connectionId, string name, int userId)
    {
        User? user = await GetUnitOfWork().UserRepository.GetUserByIdAsync(userId);
        if (user is null)
        {
            logger.LogWarning("User with id {UserId} not found", userId);
            throw new ArgumentException($"User with id {userId} does not exist.");
        }
        
        string lobbyId = Guid.NewGuid().ToString();
        Lobby lobby = new (lobbyId, name, 1);

        using (await _lobbiesLock.WriterLockAsync())
        {
            _lobbies[lobbyId] = lobby;
            _lobbyUsers[lobbyId] = [user.Username];
            _userLobbies[userId] = lobbyId;
            _connections[user.Username] = connectionId;
            _connectionUsers[connectionId] = userId;
        }
        
        logger.LogInformation("Lobby {LobbyId} created by user {Username}", lobbyId, user.Username);
        
        return lobby;
    }

    public async ValueTask<OneOf<Success<(IReadOnlyCollection<string> connections, string username)>, NotFound>> 
        JoinLobbyAsync(string connectionId, string lobbyId, int userId)
    {
        User? user = await GetUnitOfWork().UserRepository.GetUserByIdAsync(userId);
        if (user is null)
        {
            logger.LogWarning("User with id {UserId} not found", userId);
            return new NotFound();
        }
        
        using (await _lobbiesLock.WriterLockAsync())
        {
            if (_userLobbies.ContainsKey(userId))
            {
                logger.LogWarning("User {Username} is already in a lobby", user.Username);
                return new NotFound();
            }
            
            if (!_lobbies.TryGetValue(lobbyId, out Lobby? lobby))
            {
                logger.LogWarning("Lobby with id {LobbyId} not found", lobbyId);
                return new NotFound();
            }
            
            _lobbyUsers[lobbyId].Add(user.Username);
            _userLobbies[userId] = lobbyId;
            _lobbies[lobbyId] = lobby with { UserCount = lobby.UserCount + 1 };
            _connections[user.Username] = connectionId;
            _connectionUsers[connectionId] = userId;

            return new Success<(IReadOnlyCollection<string> connections, string username)>(
             (GetConnectionIdsOfLobby(lobbyId), user.Username));
        }
    }

    public async ValueTask<OneOf<Success<(IReadOnlyCollection<string> connections, 
            string username)>, NotFound>> 
        LeaveLobbyAsync(string connectionId)
    {
        using (await _lobbiesLock.WriterLockAsync())
        {
            if (!_connectionUsers.TryGetValue(connectionId, out int userId))
            {
                logger.LogWarning("Connection {ConnectionId} is not associated with any user", connectionId);
                return new NotFound();
            }
            
            User? user = await GetUnitOfWork().UserRepository.GetUserByIdAsync(userId);
            if (user is null)
            {
                logger.LogWarning("User with id {UserId} not found", userId);
                return new NotFound();
            }
            
            if (!_userLobbies.TryGetValue(userId, out string? lobbyId))
            {
                logger.LogWarning("User {Username} is not in any lobby", user.Username);
                return new NotFound();
            }
            
            if (!_lobbies.TryGetValue(lobbyId, out Lobby? lobby))
            {
                logger.LogWarning("Lobby with id {LobbyId} not found, this should never happen", 
                                  lobbyId);
                return new NotFound();
            }

            _userLobbies.Remove(userId);
            _connections.Remove(user.Username);
            _connectionUsers.Remove(connectionId);
            
            if (lobby.UserCount <= 1)
            {
                _lobbyUsers.Remove(lobbyId);
                _lobbies.Remove(lobbyId);
            }
            else
            {
                _lobbyUsers[lobbyId].Remove(user.Username);
                _lobbies[lobbyId] = lobby with { UserCount = lobby.UserCount - 1 };
            }

            return new Success<(IReadOnlyCollection<string> connections, string username)>(
                 (GetConnectionIdsOfLobby(lobbyId), user.Username));
        }
    }

    public async ValueTask<IReadOnlyCollection<Lobby>> GetLobbiesAsync()
    {
        using (await _lobbiesLock.ReaderLockAsync())
        {
            return _lobbies.Values;
        }
    }

    public async ValueTask<IReadOnlyCollection<string>> GetUsersInLobbyAsync(string lobbyId)
    {
        using (await _lobbiesLock.ReaderLockAsync())
        {
            if (!_lobbyUsers.TryGetValue(lobbyId, out HashSet<string>? users))
            {
                logger.LogWarning("Lobby with id {LobbyId} not found", lobbyId);
                return [];
            }
            return users.ToList();
        }
    }

    public async ValueTask<OneOf<Success<(IReadOnlyCollection<string> connections, string username, string filePath)>, 
        NotFound, ILobbyService.NotAllowed>> PlaySoundAsync(int soundId)
    {
        Sound? sound = await GetUnitOfWork().SoundRepository.GetByIdAsync(soundId, true);
        if (sound is null)
        {
            logger.LogWarning("Sound with id {SoundId} not found", soundId);
            return new NotFound();
        }

        if (sound.LastTimeUsed is not null 
            && sound.LastTimeUsed.Value.Plus(Duration.FromSeconds(sound.Cooldown)) 
            > clock.GetCurrentInstant())
        {
            logger.LogWarning("Sound with id {SoundId} is on cooldown", soundId);
            return new ILobbyService.NotAllowed();
        }
        
        using (await _lobbiesLock.ReaderLockAsync())
        {
            if (!_userLobbies.TryGetValue(sound.OwnerId, out string? lobbyId))
            {
                logger.LogWarning("Owner with id {UserId} is not in any lobby", sound.OwnerId);
                return new NotFound();
            }
            
            sound.LastTimeUsed = clock.GetCurrentInstant();
            await GetUnitOfWork().SaveChangesAsync();
            
            logger.LogInformation("Sound with id {SoundId} played in lobby {LobbyId}", soundId, lobbyId);
            
            return new Success<(IReadOnlyCollection<string> connections, string username, string filePath)>(
             (GetConnectionIdsOfLobby(lobbyId).Select(u => _connections[u]).ToList(), 
              sound.Owner.Username, ((SoundTemplate)sound.Template).SoundFile.FilePath));
        }
    }

    private IReadOnlyCollection<string> GetConnectionIdsOfLobby(string lobbyId)
    {
        using (_lobbiesLock.ReaderLock())
        {
            if (!_lobbyUsers.TryGetValue(lobbyId, out HashSet<string>? users))
            {
                logger.LogWarning("Lobby with id {LobbyId} not found", lobbyId);
                return [];
            }
            return users.Select(u => _connections[u]).ToList();
        }
    }

    private IUnitOfWork GetUnitOfWork()
    {
        IUnitOfWork? uow = scopeFactory.CreateScope().ServiceProvider.GetService<IUnitOfWork>();
        if (uow is null)
        {
            throw new InvalidOperationException("Unit of Work is not registered in the service provider.");
        }
        return uow;
    }
}
