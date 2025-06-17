namespace SoundCaseOpener.Core.Logic;

public static class Util
{
    public static bool TryHitChance(double chance)
    {
        if (chance < 0 || chance > 1)
            throw new ArgumentOutOfRangeException(nameof(chance), "Chance must be between 0 and 1.");

        return Random.Shared.NextDouble() <= chance;
    }
}
