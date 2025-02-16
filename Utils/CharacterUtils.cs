using System;

namespace CharacterData.Utils;

public static class CharacterUtils
{
    public static int ResistanceDifference(int capRes, int uncappedRes, int maxCapRes)
    {
        return uncappedRes <= capRes ? capRes - maxCapRes : uncappedRes - capRes;
    }

    public static double CalculateProgress(int level, long experience)
    {
        return level != 100
            ? (experience - (double)PlayerExperience.TotalExperience[level])
            / PlayerExperience.NextExperience[level] * 100.0
            : 0.0;
    }

    public static double GetLevelGainPercent(int level, long xpGained)
    {
        return level == 100
            ? 0.0
            : Math.Round(xpGained * 100.0 / PlayerExperience.NextExperience[level], 2);
    }

    public static int? GetRunsToNextLevel(int level, long xp, long xpGained)
    {
        if (xpGained <= 0 || level >= 100)
            return null;
        var progress = CalculateProgress(level, xp);
        var runs = Math.Round(
            (100.0 - progress) / 100.0 * PlayerExperience.NextExperience[level] / xpGained,
            0
        );
        return !double.IsInfinity(runs) ? (int)runs : null;
    }

    public static int? GetTotalRuns(int level, long xpGained)
    {
        if (xpGained <= 0 || level >= 100)
            return null;
        var runs = Math.Round(PlayerExperience.NextExperience[level] / (double)xpGained, 0);
        return !double.IsInfinity(runs) ? (int)runs : null;
    }

    public static double? GetTimeToLevelSeconds(long xpGained, double timeElapsed,
        int level, long playerXp)
    {
        if (xpGained <= 0 || timeElapsed <= 0 || level >= 100)
            return null;
        var xpPerSecond = xpGained / timeElapsed;
        if (xpPerSecond <= 0)
            return null;
        double totalNeeded = PlayerExperience.NextExperience[level];
        double currentExpInLevel = playerXp - PlayerExperience.TotalExperience[level];
        var remaining = totalNeeded - currentExpInLevel;
        var timeNeededSecs = remaining / xpPerSecond;
        return Math.Round(timeNeededSecs, 2);
    }

    public static string FormatTime(double seconds)
    {
        var t = TimeSpan.FromSeconds(seconds);
        return t.TotalHours >= 1
            ? $"{(int)t.TotalHours}h {t.Minutes}m"
            : $"{t.Minutes}m {t.Seconds}s";
    }

    public static string FormatElapsedTime(double timeElapsed)
    {
        var t = TimeSpan.FromSeconds(timeElapsed);
        return t.TotalHours >= 1
            ? $"{(int)t.TotalHours}:{t.Minutes:00}:{t.Seconds:00}"
            : $"{t.Minutes}:{t.Seconds:00}";
    }
}