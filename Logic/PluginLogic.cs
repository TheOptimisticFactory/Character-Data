using System;
using System.Collections.Generic;
using CharacterData.Export;
using CharacterData.Structs;
using CharacterData.Utils;
using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.Shared.Enums;

namespace CharacterData.NewFolder;

public static class PluginLogic
{
    // These fields now hold the logic state previously inside Main.
    private static bool _initialised;
    private static InstanceData _currentInstance;
    private static readonly List<SnapshotData> _snapshots = new();

    // Expose some state for rendering purposes.
    public static bool WaitingForPlayer { get; private set; }

    public static bool PendingAreaReset { get; private set; }

    public static SnapshotData CurrentSnapshot { get; private set; }

    public static void Initialise()
    {
        _initialised = false;
        WaitingForPlayer = true;
        PendingAreaReset = false;
    }

    public static void Update()
    {
        // If waiting for the player or the area just reset, update the session start.
        if (WaitingForPlayer || PendingAreaReset)
        {
            var player = Main.Plugin.GameController.Player?.GetComponent<Player>();
            if (player != null)
            {
                var currentKills = TryGetStat(GameStat.CharacterKillCount);
                long currentXp = player.XP;
                _currentInstance = new InstanceData(
                    Main.Plugin.GameController.Area.CurrentArea,
                    currentXp, currentKills, DateTime.Now);

                if (WaitingForPlayer)
                {
                    WaitingForPlayer = false;
                    _initialised = true;
                }

                if (PendingAreaReset)
                    PendingAreaReset = false;
            }
        }
        else
        {
            CurrentSnapshot = CreateSnapshot();
        }
    }

    public static void AreaChange(AreaInstance area)
    {
        if (_initialised)
        {
            if (ShouldLog())
            {
                // Delegate exporting to the SnapshotExporter helper.
                SnapshotExporter.ExportSnapshot(CurrentSnapshot);
                if (_snapshots.Count >= 10)
                    _snapshots.RemoveAt(0);
                _snapshots.Add(CurrentSnapshot);
            }

            WaitingForPlayer = true;
        }
    }

    private static int TryGetStat(GameStat stat)
    {
        return Main.Plugin.GameController.Player.Stats.GetValueOrDefault(stat, 0);
    }

    private static bool ShouldLog()
    {
        if (!Main.Plugin.Settings.SnapshotSettings.Enabled)
            return false;

        if (Main.Plugin.Settings.SnapshotSettings.LogAllAreaChanges)
            return true;

        var player = Main.Plugin.GameController.Player?.GetComponent<Player>();
        if (player == null)
            return false;

        var currentKills = TryGetStat(GameStat.CharacterKillCount);
        var xpGained = player.XP - _currentInstance.JoinExperience;
        var killsGained = currentKills - _currentInstance.JoinKills;
        return xpGained > 0 || killsGained > 0;
    }

    private static SnapshotData CreateSnapshot()
    {
        var player = Main.Plugin.GameController.Player?.GetComponent<Player>();
        if (player == null)
            return null;

        var currentKills = TryGetStat(GameStat.CharacterKillCount);
        long currentXp = player.XP;
        var progressPct = CharacterUtils.CalculateProgress(player.Level, player.XP);
        var xpGained = currentXp - _currentInstance.JoinExperience;
        var levelPercent = CharacterUtils.GetLevelGainPercent(player.Level, xpGained);
        var timeElapsed = (DateTime.Now - _currentInstance.JoinTime).TotalSeconds;
        var xpPerHour = timeElapsed > 0 ? xpGained / timeElapsed * 3600 : 0.0;

        var runsToNext = CharacterUtils.GetRunsToNextLevel(player.Level, player.XP, xpGained);
        var totalRuns = CharacterUtils.GetTotalRuns(player.Level, xpGained);
        var timeToLevelSecs = CharacterUtils.GetTimeToLevelSeconds(
            xpGained, timeElapsed, player.Level, player.XP);

        var areaDiff = player.Level - Main.Plugin.GameController.Game.IngameState.Data.CurrentAreaLevel;
        var areaKills = currentKills - _currentInstance.JoinKills;

        var xpPerMobAvg = areaKills > 0 ? (double)xpGained / areaKills : (double?)null;

        var physReduction = TryGetStat(GameStat.DisplayEstimatedPhysicalDamageReducitonPct);
        var armor = TryGetStat(GameStat.PhysicalDamageReductionRating);
        var evasion = TryGetStat(GameStat.EvasionRating);
        var evadeChance = TryGetStat(GameStat.ChanceToEvadePct);
        var blockChance = TryGetStat(GameStat.AttackBlockPct);

        var fireRes = TryGetStat(GameStat.FireDamageResistancePct);
        var fireResTotal = TryGetStat(GameStat.UncappedFireDamageResistancePct);
        var maxFireRes = TryGetStat(GameStat.MaximumFireDamageResistancePct) != 0
            ? TryGetStat(GameStat.MaximumFireDamageResistancePct)
            : 75;
        var fireDiff = CharacterUtils.ResistanceDifference(fireRes, fireResTotal, maxFireRes);

        var coldRes = TryGetStat(GameStat.ColdDamageResistancePct);
        var coldResTotal = TryGetStat(GameStat.UncappedColdDamageResistancePct);
        var maxColdRes = TryGetStat(GameStat.MaximumColdDamageResistancePct) != 0
            ? TryGetStat(GameStat.MaximumColdDamageResistancePct)
            : 75;
        var coldDiff = CharacterUtils.ResistanceDifference(coldRes, coldResTotal, maxColdRes);

        var lightningRes = TryGetStat(GameStat.LightningDamageResistancePct);
        var lightningResTotal = TryGetStat(GameStat.UncappedLightningDamageResistancePct);
        var maxLightningRes = TryGetStat(GameStat.MaximumLightningDamageResistancePct) != 0
            ? TryGetStat(GameStat.MaximumLightningDamageResistancePct)
            : 75;
        var lightningDiff = CharacterUtils.ResistanceDifference(
            lightningRes, lightningResTotal, maxLightningRes);

        var chaosRes = TryGetStat(GameStat.ChaosDamageResistancePct);
        var chaosResTotal = TryGetStat(GameStat.UncappedChaosDamageResistancePct);
        var maxChaosRes = TryGetStat(GameStat.MaximumChaosDamageResistancePct) != 0
            ? TryGetStat(GameStat.MaximumChaosDamageResistancePct)
            : 75;
        var chaosDiff = CharacterUtils.ResistanceDifference(chaosRes, chaosResTotal, maxChaosRes);

        return new SnapshotData
        {
            SnapshotTime = DateTimeOffset.Now.ToUnixTimeSeconds(),
            Area = new AreaData
            {
                Name = _currentInstance.Area.Name,
                Level = _currentInstance.Area.RealLevel,
                Act = _currentInstance.Area.Act,
                Difference = areaDiff
            },
            Player = new PlayerData
            {
                Level = player.Level,
                Xp = currentXp,
                XpData = new XpData
                {
                    ProgressPercent = progressPct,
                    XpGained = xpGained,
                    LevelPercent = levelPercent,
                    XpPerHour = xpPerHour,
                    XpPerMobAvg = xpPerMobAvg,
                    TimeToLevelSeconds = timeToLevelSecs,
                    AreaTimeSeconds = timeElapsed
                },
                Runs = new RunsData
                {
                    RunsToNext = runsToNext,
                    TotalRuns = totalRuns
                },
                Kills = new KillsData
                {
                    Total = currentKills,
                    Area = areaKills
                }
            },
            Resistances = new ResistanceData
            {
                Fire = new ResistanceDetail
                {
                    Capped = fireRes,
                    Uncapped = fireResTotal,
                    Max = maxFireRes,
                    Diff = fireDiff
                },
                Cold = new ResistanceDetail
                {
                    Capped = coldRes,
                    Uncapped = coldResTotal,
                    Max = maxColdRes,
                    Diff = coldDiff
                },
                Lightning = new ResistanceDetail
                {
                    Capped = lightningRes,
                    Uncapped = lightningResTotal,
                    Max = maxLightningRes,
                    Diff = lightningDiff
                },
                Chaos = new ResistanceDetail
                {
                    Capped = chaosRes,
                    Uncapped = chaosResTotal,
                    Max = maxChaosRes,
                    Diff = chaosDiff
                }
            },
            Defenses = new DefenseData
            {
                Armor = new ArmorData
                {
                    Rating = armor,
                    DisplayReduction = physReduction
                },
                Evasion = new EvasionData
                {
                    Rating = evasion,
                    ChanceToEvade = evadeChance
                },
                Block = new BlockData
                {
                    AttackBlockPct = blockChance
                }
            }
        };
    }

    // Record struct moved here from Main.
    public readonly record struct InstanceData(
        AreaInstance Area,
        long JoinExperience,
        int JoinKills,
        DateTime JoinTime);
}