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
            ? (experience - (double)PlayerExperience.TotalExperience[level]) / PlayerExperience.NextExperience[level] *
              100.0
            : 0.0;
    }

    public static string CalculateLevelPercent(int level, long gained)
    {
        return level == 100 ? "0.0" : (gained * 100.0 / PlayerExperience.NextExperience[level]).ToString("N2");
    }

    public static string CalculateRunsToNext(int level, long exp, long gained)
    {
        if (gained <= 0) return "∞";
        var progress = CalculateProgress(level, exp);
        var d = Math.Round((100.0 - progress) / 100.0 * PlayerExperience.NextExperience[level] / gained, 0);
        return !double.IsInfinity(d) ? d.ToString("N0") : "∞";
    }

    public static string CalculateTotalRuns(int level, long gained)
    {
        if (gained <= 0) return "∞";
        var d = Math.Round(PlayerExperience.NextExperience[level] / (double)gained, 2);
        return !double.IsInfinity(d) ? d.ToString("N0") : "∞";
    }

    public static string CalculateTimeToLevel(long expGained, double timeElapsed, int playerLevel, long playerXP)
    {
        if (expGained <= 0 || timeElapsed <= 0) return "∞";
        if (playerLevel >= 100) return "∞";

        var xpPerHour = expGained / timeElapsed * 3600;
        if (xpPerHour <= 0) return "∞";

        var totalNeeded = PlayerExperience.NextExperience[playerLevel];
        var remaining = totalNeeded - (playerXP - PlayerExperience.TotalExperience[playerLevel]);
        var timeNeeded = remaining / xpPerHour * 3600;
        var hours = (int)(timeNeeded / 3600);
        var minutes = (int)(timeNeeded % 3600 / 60);
        var seconds = (int)(timeNeeded % 60);

        return hours > 0
            ? $"{hours}h {minutes}m"
            : $"{minutes}m {seconds}s";
    }

    public static string FormatElapsedTime(double timeElapsed)
    {
        var hours = (int)(timeElapsed / 3600);
        var minutes = (int)(timeElapsed % 3600 / 60);
        var seconds = (int)(timeElapsed % 60);

        return hours > 0
            ? $"{hours}:{minutes:00}:{seconds:00}"
            : $"{minutes}:{seconds:00}";
    }
}