namespace SoundCaseOpener.Shared;

public interface ILobbyHub
{
    public const string Route = "hub/lobby";

    public ValueTask<Lobby> CreateLobbyAsync(string name, int userId);
    public ValueTask<bool> JoinLobbyAsync(string lobbyId, int userId);
    public ValueTask<bool> LeaveLobbyAsync();
    public ValueTask<IReadOnlyCollection<Lobby>> GetLobbiesAsync();
    public ValueTask<IReadOnlyCollection<string>> GetUsersInLobbyAsync(string lobbyId);

    public ValueTask<bool> PlaySoundAsync(int soundId);
}

public interface ILobbyHubClient
{
    public Task ReceiveLobbyCreatedAsync(Lobby lobby);
    public Task ReceiveLobbyClosedAsync(string lobbyId);
    public Task ReceiveUserJoinedLobbyAsync(string username);
    public Task ReceiveUserLeftLobbyAsync(string username);
    public Task ReceiveUserPlayedSoundAsync(string username, string filePath);
}