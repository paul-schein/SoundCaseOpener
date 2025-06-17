using SoundCaseOpener.Persistence.Model;

namespace SoundCaseOpener.Core.Logic;

public static class ExtensionMethods
{
    public static T GetRandomElement<T>(this List<T> source)
    {
        if (!source.Any())
        {
            throw new InvalidOperationException("Cannot get a random element from an empty collection.");
        }

        return source[Random.Shared.Next(0, source.Count)];
    }
    
    public static Sound ToRandomSound(this SoundTemplate template, User user) =>
        new()
        {
            Name = template.Name,
            Cooldown = Random.Shared.Next(template.MinCooldown, template.MaxCooldown + 1),
            Template = template,
            Owner = user
        };
    
    public static Case ToCase(this CaseTemplate template, User user) =>
        new()
        {
            Name = template.Name,
            Owner = user,
            Template = template
        };
}