using Microsoft.AspNetCore.SignalR;
using OneOf;
using OneOf.Types;
using SoundCaseOpener.Core.Services;
using SoundCaseOpener.Persistence.Util;
using SoundCaseOpener.Shared;

namespace SoundCaseOpener.Hubs;

public sealed class LobbyHub(ILobbyService lobbyService,
                             ITransactionProvider transaction,
                             ILogger<LobbyHub> logger) : Hub<ILobbyHubClient>, ILobbyHub
{
    public async ValueTask<Lobby> CreateLobbyAsync(string name, int userId)
    {
        if (userId <= 0)
        {
            logger.LogWarning("Invalid user id {UserId} received", userId);
            throw new ArgumentException("User id must be greater than 0", nameof(userId));
        }
        
        Lobby lobby =  await lobbyService.CreateLobbyAsync(Context.ConnectionId, name, userId);

        await Clients.Others.ReceiveLobbyCreatedAsync(lobby);
        
        logger.LogInformation("Sent lobby created event to all other clients for lobby {LobbyId}", lobby.Id);
        
        return lobby;
    }

    public async ValueTask<bool> JoinLobbyAsync(string lobbyId, int userId)
    {
        if (userId <= 0)
        {
            logger.LogWarning("Invalid user id {UserId} received", userId);
            return false;
        }
        
        if (string.IsNullOrWhiteSpace(lobbyId))
        {
            logger.LogWarning("Invalid lobby id {LobbyId} received", lobbyId);
            return false;
        }
        
        OneOf<Success<(IReadOnlyCollection<string> connections, string username)>, NotFound> result = 
            await lobbyService.JoinLobbyAsync(Context.ConnectionId, lobbyId, userId);
        
        return result.Match<bool>(
            success =>
            {
                Clients.Clients(success.Value.connections)
                       .ReceiveUserJoinedLobbyAsync(success.Value.username);
                logger.LogInformation("Sent user joined lobby event to all other clients for lobby {LobbyId}", lobbyId);
                return true;
            },
            notFound => false);
    }

    public async ValueTask<bool> LeaveLobbyAsync()
    {
        OneOf<Success<(IReadOnlyCollection<string> connections, string username)>, NotFound> result = 
            await lobbyService.LeaveLobbyAsync(Context.ConnectionId);

        return result.Match<bool>(success =>
        {
            Clients.Clients(success.Value.connections)
                   .ReceiveUserLeftLobbyAsync(success.Value.username);
            logger.LogInformation("Sent user left lobby event to all other clients for lobby {LobbyId}", 
                                  success.Value.username);
            return true;
        },
        notFound => false);
    }

    public async ValueTask<IReadOnlyCollection<Lobby>> GetLobbiesAsync() => 
        await lobbyService.GetLobbiesAsync();

    public async ValueTask<IReadOnlyCollection<string>> GetUsersInLobbyAsync(string lobbyId) => 
        await lobbyService.GetUsersInLobbyAsync(lobbyId);

    public async ValueTask<bool> PlaySoundAsync(int soundId)
    {
        if (soundId <= 0)
        {
            logger.LogWarning("Invalid sound id {SoundId} received", soundId);
            return false;
        }

        try
        {
            await transaction.BeginTransactionAsync();
            
            OneOf<Success<(IReadOnlyCollection<string> connections, string username, string filePath)>, 
                NotFound, ILobbyService.NotAllowed> result = 
                await lobbyService.PlaySoundAsync(soundId);
            
            return result.Match<bool>(
                success =>
                {
                    Clients.Clients(success.Value.connections)
                           .ReceiveUserPlayedSoundAsync(success.Value.username, 
                                                        success.Value.filePath);
                    logger.LogInformation("Sent user played sound event to all other clients for sound {SoundId}", 
                                          soundId);
                    return true;
                },
                notFound => false,
                notAllowed => false);
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            logger.LogError(e, "Error while playing sound {SoundId}", soundId);
            return false;
        }
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await LeaveLobbyAsync();
        await base.OnDisconnectedAsync(exception);
    }
}
