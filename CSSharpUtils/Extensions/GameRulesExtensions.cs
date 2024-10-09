using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;

namespace CSSharpUtils.Extensions;

/// <summary>
/// Provides extension methods for <see cref="CCSGameRules"/> to enhance functionality.
/// </summary>
public static class GameRulesExtensions
{
    private static ConVar? mp_halftime;
    private static ConVar? mp_maxrounds;

    /// <summary>
    /// Calculates the remaining time in the current round.
    /// </summary>
    /// <param name="gameRules">The game rules instance, or null if not available.</param>
    /// <returns>The remaining time in seconds; returns 0.0f if <paramref name="gameRules"/> is null.</returns>
    public static float GetRemainingRoundTime(this CCSGameRules? gameRules)
    {
        if (gameRules == null)
            return 0.0f;

        // Calculate remaining time by subtracting the current time from the sum of round start time and round duration
        return (gameRules.RoundStartTime + gameRules.RoundTime) - Server.CurrentTime;
    }

    /// </summary>
    /// Gets if it is warmup or not.
    /// </summary>
    /// <param name="gameRules">The game rules instance, or null if not available.</param>
    /// <returns>It is warmup or not; returns false if <paramref name="gameRules"/> is null.</returns>
    public static bool IsWarmup(this CCSGameRules? gameRules)
    {
        return gameRules?.WarmupPeriod ?? false;
    }

    /// </summary>
    /// Gets if it is pistol round or not.
    /// </summary>
    /// <param name="gameRules">The game rules instance, or null if not available.</param>
    /// <returns>It is pistol round or not; returns false if <paramref name="gameRules"/> is null.</returns>
    public static bool IsPistolRound(this CCSGameRules? gameRules)
    {
        if (gameRules == null)
            return false;

        mp_halftime ??= ConVar.Find("mp_halftime")!;
        mp_maxrounds ??= ConVar.Find("mp_maxrounds")!;

        bool halftime = mp_halftime.GetPrimitiveValue<bool>();
        int maxrounds = mp_maxrounds.GetPrimitiveValue<int>();

        return gameRules.TotalRoundsPlayed == 0 ||
               (halftime && maxrounds / 2 == gameRules.TotalRoundsPlayed) ||
               gameRules.GameRestart;
    }
}