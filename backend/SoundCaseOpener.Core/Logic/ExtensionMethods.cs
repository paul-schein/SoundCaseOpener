using SoundCaseOpener.Persistence.Model;

namespace SoundCaseOpener.Core.Logic;

public static class ExtensionMethods
{
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