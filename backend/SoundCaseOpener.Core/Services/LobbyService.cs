using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nito.AsyncEx;
using OneOf;
using OneOf.Types;
using SoundCaseOpener.Core.Logic;
using SoundCaseOpener.Core.Util;
using SoundCaseOpener.Persistence.Model;
using SoundCaseOpener.Persistence.Util;
using SoundCaseOpener.Shared;

namespace SoundCaseOpener.Core.Services;

public interface ILobbyService
{
    public ValueTask<OneOf<Success<Lobby>, ILobbyService.NotAllowed>> 
        CreateLobbyAsync(string connectionId, string name, int userId);
    public ValueTask<OneOf<Success<(IReadOnlyCollection<string> connections, string username, 
            string lobbyId)>, NotFound>> 
        JoinLobbyAsync(string connectionId, string lobbyId, int userId);
    public ValueTask<OneOf<Success<(IReadOnlyCollection<string> connections, string username, 
            string lobbyId, bool lobbyDeleted)>, NotFound>> 
        LeaveLobbyAsync(string connectionId);
    public ValueTask<IReadOnlyCollection<Lobby>> GetLobbiesAsync();
    public ValueTask<IReadOnlyCollection<string>> GetUsersInLobbyAsync(string lobbyId);
    public ValueTask<OneOf<
            Success<UsersSoundPlayed>, 
            SuccessCaseObtained,
            NotFound, NotAllowed>> 
        PlaySoundAsync(int soundId, string connectionId);
    public ValueTask<OneOf<Lobby, NotFound>> GetLobbyByIdAsync(string lobbyId);
    
    public readonly record struct NotAllowed;
    public readonly record struct SuccessCaseObtained(UsersSoundPlayed UsersSoundPlayed, UserCases UserCases);
    
    public sealed record UsersSoundPlayed(
        IReadOnlyCollection<string> Connections, 
        string Username, 
        string FilePath);
    
    public sealed record UserCases(IReadOnlyCollection<(string connectionId, int caseId)> Connections);
}

public class LobbyService(IServiceScopeFactory scopeFactory, 
                          IClock clock,
                          IOptions<Settings> settings,
                          ILogger<LobbyService> logger) : ILobbyService
{
    private readonly Dictionary<string, Lobby> _lobbies = [];
    private readonly Dictionary<string, HashSet<string>> _lobbyUsers = [];
    private readonly Dictionary<int, string> _userLobbies = [];
    private readonly Dictionary<string, string> _connections = [];
    private readonly Dictionary<string, int> _connectionUsers = [];
    private readonly AsyncReaderWriterLock _lobbiesLock = new();

    public async ValueTask<OneOf<Success<Lobby>, ILobbyService.NotAllowed>>
        CreateLobbyAsync(string connectionId, string name, int userId)
    {
        if (_connectionUsers.ContainsKey(connectionId))
        {
            logger.LogWarning("Connection {ConnectionId} is already associated with a user", connectionId);
            return new ILobbyService.NotAllowed();
        }
        
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
            TryConnectUser(connectionId, user);
            _lobbies[lobbyId] = lobby;
            _lobbyUsers[lobbyId] = [user.Username];
            _userLobbies[userId] = lobbyId;
        }
        
        logger.LogInformation("Lobby {LobbyId} created by user {Username}", lobbyId, user.Username);
        
        return new Success<Lobby>(lobby);
    }

    public async ValueTask<OneOf<Success<(IReadOnlyCollection<string> connections, 
            string username, string lobbyId)>, NotFound>> 
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
                logger.LogInformation("User {Username} is already in a lobby", user.Username);
                return new NotFound();
            }
            
            if (!_lobbies.TryGetValue(lobbyId, out Lobby? lobby))
            {
                logger.LogWarning("Lobby with id {LobbyId} not found", lobbyId);
                return new NotFound();
            }
            
            TryConnectUser(connectionId, user);
            _lobbyUsers[lobbyId].Add(user.Username);
            _userLobbies[userId] = lobbyId;
            _lobbies[lobbyId] = lobby with { UserCount = lobby.UserCount + 1 };
            
            logger.LogInformation("User {Username} joined lobby {LobbyId}", user.Username, lobbyId);

            return new Success<(IReadOnlyCollection<string> connections, string username, string lobbyId)>(
             (GetConnectionIdsOfLobby(lobbyId), user.Username, lobbyId));
        }
    }

    public async ValueTask<OneOf<Success<(IReadOnlyCollection<string> connections, 
            string username, string lobbyId, bool lobbyDeleted)>, NotFound>> 
        LeaveLobbyAsync(string connectionId)
    {
        using (await _lobbiesLock.WriterLockAsync())
        {
            if (!_connectionUsers.TryGetValue(connectionId, out int userId))
            {
                logger.LogInformation("Connection {ConnectionId} is not associated with any user", connectionId);
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
            
            bool lobbyDeleted = lobby.UserCount <= 1;
            if (lobbyDeleted)
            {
                _lobbyUsers.Remove(lobbyId);
                _lobbies.Remove(lobbyId);
                logger.LogInformation("Lobby {LobbyId} deleted as it had no users left", lobbyId);
            }
            else
            {
                _lobbyUsers[lobbyId].Remove(user.Username);
                _lobbies[lobbyId] = lobby with { UserCount = lobby.UserCount - 1 };
                logger.LogInformation("User {Username} left lobby {LobbyId}", user.Username, lobbyId);
            }

            TryDisconnectUser(connectionId, user.Username);
            
            return new Success<(IReadOnlyCollection<string> connections, string username, 
                string lobbyId, bool lobbyDeleted)>(
                 (GetConnectionIdsOfLobby(lobbyId), user.Username, lobbyId, lobbyDeleted));
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

    public async ValueTask<OneOf<Success<ILobbyService.UsersSoundPlayed>, 
            ILobbyService.SuccessCaseObtained,
            NotFound, ILobbyService.NotAllowed>> PlaySoundAsync(int soundId, string connectionId)
    {
        IUnitOfWork uow = GetUnitOfWork();
        Sound? sound = await uow.SoundRepository.GetByIdAsync(soundId, true);
        if (sound is null)
        {
            logger.LogWarning("Sound with id {SoundId} not found", soundId);
            return new NotFound();
        }

        if (sound.LastTimeUsed is not null 
            && sound.LastTimeUsed.Value.Plus(Duration.FromSeconds(sound.Cooldown)) 
            > clock.GetCurrentInstant())
        {
            logger.LogInformation("Sound with id {SoundId} is on cooldown", soundId);
            return new ILobbyService.NotAllowed();
        }
        
        using (await _lobbiesLock.ReaderLockAsync())
        {
            if (!_userLobbies.TryGetValue(sound.OwnerId, out string? lobbyId))
            {
                logger.LogWarning("Owner with id {UserId} is not in any lobby", sound.OwnerId);
                return new NotFound();
            }

            if (!_connectionUsers.TryGetValue(connectionId, out int userId))
            {
                logger.LogWarning("Connection {ConnectionId} is not associated with any user", connectionId);
                return new ILobbyService.NotAllowed();
            }
            
            if (sound.OwnerId != userId)
            {
                logger.LogWarning("User {Username} is not the owner of sound with id {SoundId}", 
                                  _connections[connectionId], soundId);
                return new ILobbyService.NotAllowed();
            }
            
            sound.LastTimeUsed = clock.GetCurrentInstant();
            
            logger.LogInformation("Sound with id {SoundId} played in lobby {LobbyId}", soundId, lobbyId);

            ILobbyService.UsersSoundPlayed usersSoundPlayed
                = new(GetConnectionIdsOfLobby(lobbyId),
                      sound.Owner.Username, ((SoundTemplate) sound.Template).SoundFile.FilePath);
            
            if (Logic.Util.TryHitChance(settings.Value.RandomCaseChance))
            {
                return new ILobbyService.SuccessCaseObtained(usersSoundPlayed,
                    await CreateCasesForUsers(
                    (await uow.CaseTemplateRepository.GetAllAsync(true)).ToList().GetRandomElement(),
                        await uow.UserRepository.GetUsersByUsernameAsync(_lobbyUsers[lobbyId]),
                    uow));
            }

            await uow.SaveChangesAsync();
            return new Success<ILobbyService.UsersSoundPlayed>(usersSoundPlayed);
        }
    }

    public async ValueTask<OneOf<Lobby, NotFound>> GetLobbyByIdAsync(string lobbyId)
    {
        using (await _lobbiesLock.ReaderLockAsync())
        {
            if (!_lobbies.TryGetValue(lobbyId, out Lobby? lobby))
            {
                logger.LogInformation("Lobby with id {LobbyId} not found", lobbyId);
                return new NotFound();
            }
            return lobby;
        }
    }

    private bool TryDisconnectUser(string connectionId, string username)
    {
        if (!_connectionUsers.Remove(connectionId, out int userId))
        {
            logger.LogInformation("Connection {ConnectionId} is not associated with any user", connectionId);
            return false;
        }

        _connections.Remove(username);
        logger.LogInformation("Disconnected user {Username} from connection {ConnectionId}",
                              username, connectionId);
        return true;
    }

    private bool TryConnectUser(string connectionId, User user)
    {
        if (!_connections.TryAdd(user.Username, connectionId))
        {
            logger.LogInformation("User {Username} is already connected", user.Username);
            return false;
        }

        _connectionUsers[connectionId] = user.Id;
        logger.LogInformation("Connected user {Username} to connection {ConnectionId}", 
                              user.Username, connectionId);
        return true;
    }
    
    private async ValueTask<ILobbyService.UserCases> CreateCasesForUsers(CaseTemplate template,
                                                                         IReadOnlyCollection<User> users, 
                                                                         IUnitOfWork uow)
    {
        List<Case> cases = [];
        foreach (User user in users)
        {
            Case caseItem = template.ToCase(user);
            uow.CaseRepository.Add(caseItem);
            cases.Add(caseItem);
        }

        await uow.SaveChangesAsync();

        return new ILobbyService.UserCases(cases.Select(c => 
                                                            (_connections[c.Owner.Username], c.Id)).ToList());
    }
    
    private IReadOnlyCollection<string> GetConnectionIdsOfLobby(string lobbyId)
    {
        if (!_lobbyUsers.TryGetValue(lobbyId, out HashSet<string>? users))
        {
            logger.LogInformation("Lobby with id {LobbyId} not found", lobbyId);
            return [];
        }
        return users.Select(u => _connections[u]).ToList();
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
