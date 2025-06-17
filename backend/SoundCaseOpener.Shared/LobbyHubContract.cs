namespace SoundCaseOpener.Shared;

public interface ILobbyHub
{
    public const string Route = "hub/lobby";

    public ValueTask<int> CreateLobbyAsync(string name);
    public ValueTask<bool> JoinLobbyAsync(int lobbyId, int userId);
    public ValueTask<bool> LeaveLobbyAsync(int lobbyId, int userId);
    public ValueTask<IReadOnlyCollection<Lobby>> GetLobbiesAsync();
    public ValueTask<IReadOnlyCollection<string>> GetUsersInLobbyAsync(int lobbyId);

    public ValueTask<bool> PlaySoundAsync(int soundId);
}

public interface ILobbyHubClient
{
    public Task ReceiveUserJoinedLobby(string username);
    public Task ReceiveUserLeftLobby(string username);
    public Task ReceiveUserPlayedSound(string filePath);
}