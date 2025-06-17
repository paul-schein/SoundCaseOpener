using SoundCaseOpener.Shared;

namespace SoundCaseOpener.Persistence.Model;

public class User
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public required Role Role { get; set; }
    public required List<Item> Items { get; set; }
}
