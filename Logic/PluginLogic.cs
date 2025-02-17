using System;
using System.Collections.Generic;
using CharacterData.Structs;
using CharacterData.Utils;
using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.Shared.Enums;
using static CharacterData.Main;

namespace CharacterData.Logic;

public static class PluginLogic
{
    private static bool _initialised;
    private static InstanceData _currentInstance;
    private static readonly List<SnapshotData> Snapshots = [];

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
        if (Plugin?.GameController?.Player == null)
        {
            WaitingForPlayer = true;
            return;
        }

        if (WaitingForPlayer || PendingAreaReset)
        {
            var player = Plugin.GameController.Player.GetComponent<Player>();
            if (player != null)
            {
                var currentKills = TryGetStat(GameStat.CharacterKillCount);
                long currentXp = player.XP;

                if (Plugin.GameController.Area?.CurrentArea != null)
                {
                    _currentInstance = new InstanceData(
                        player.PlayerName,
                        Plugin.GameController.Area.CurrentArea,
                        currentXp,
                        currentKills,
                        DateTime.Now);

                    if (WaitingForPlayer)
                    {
                        WaitingForPlayer = false;
                        _initialised = true;
                    }

                    if (PendingAreaReset)
                        PendingAreaReset = false;
                }
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
                CurrentSnapshot.EndArea = new AreaData
                {
                    Name = area.Name,
                    Level = area.RealLevel,
                    Act = area.Act,
                    Difference = CurrentSnapshot.Player.Level - area.RealLevel
                };

                Plugin.ExportManager.ExportSnapshot(CurrentSnapshot, _currentInstance.CharacterName);
                if (Snapshots.Count >= 10)
                    Snapshots.RemoveAt(0);
                Snapshots.Add(CurrentSnapshot);
            }

            WaitingForPlayer = true;
        }
    }

    private static int TryGetStat(GameStat stat)
    {
        return Plugin?.GameController?.Player?.Stats?.GetValueOrDefault(stat, 0) ?? 0;
    }

    private static bool ShouldLog()
    {
        if (!Plugin.Settings.InstanceExportSettings.Enabled)
            return false;

        if (_currentInstance.CharacterName != Plugin.GameController?.Player?.GetComponent<Player>().PlayerName)
            return false;

        if (_currentInstance.Area.IsPeaceful && !Plugin.Settings.InstanceExportSettings.EnablePeacefulAreas)
            return false;

        if (Plugin.Settings.InstanceExportSettings.DisableConditionalShouldLogChecks)
            return true;

        var player = Plugin.GameController?.Player?.GetComponent<Player>();
        if (player == null)
            return false;

        var currentKills = TryGetStat(GameStat.CharacterKillCount);
        var xpGained = player.XP - _currentInstance.JoinExperience;
        var killsGained = currentKills - _currentInstance.JoinKills;
        return xpGained > 0 || killsGained > 0;
    }

    private static SnapshotData CreateSnapshot()
    {
        var playerComp = Plugin.GameController.Player?.GetComponent<Player>();
        if (playerComp == null)
        {
            Plugin.LogError("[CreateSnapshot] playerComp == null", 10);
            return null;
        }

        var lifeComp = Plugin.GameController.Player?.GetComponent<Life>();
        if (lifeComp == null)
        {
            Plugin.LogError("[CreateSnapshot] lifeComp == null", 10);
            return null;
        }

        var currentKills = TryGetStat(GameStat.CharacterKillCount);
        long currentXp = playerComp.XP;
        var progressPct = CharacterUtils.CalculateProgress(playerComp.Level, playerComp.XP);
        var xpGained = currentXp - _currentInstance.JoinExperience;
        var levelPercent = CharacterUtils.GetLevelGainPercent(playerComp.Level, xpGained);
        var timeElapsed = (DateTime.Now - _currentInstance.JoinTime).TotalSeconds;
        var xpPerHour = timeElapsed > 0 ? xpGained / timeElapsed * 3600 : 0.0;

        var runsToNext = CharacterUtils.GetRunsToNextLevel(playerComp.Level, playerComp.XP, xpGained);
        var totalRuns = CharacterUtils.GetTotalRuns(playerComp.Level, xpGained);
        var timeToLevelSecs = CharacterUtils.GetTimeToLevelSeconds(
            xpGained, timeElapsed, playerComp.Level, playerComp.XP);

        var areaDiff = playerComp.Level - _currentInstance.Area.RealLevel;
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
            AreaTimeSeconds = timeElapsed,
            StartArea = new AreaData
            {
                Name = _currentInstance.Area.Name,
                Level = _currentInstance.Area.RealLevel,
                Act = _currentInstance.Area.Act,
                Difference = areaDiff
            },
            Player = new PlayerData
            {
                Level = playerComp.Level,
                Xp = currentXp,
                MaxHP = lifeComp.MaxHP,
                MaxES = lifeComp.MaxES,
                MaxMana = lifeComp.MaxMana,
                XpData = new XpData
                {
                    ProgressPercent = progressPct,
                    XpGained = xpGained,
                    LevelPercent = levelPercent,
                    XpPerHour = xpPerHour,
                    XpPerMobAvg = xpPerMobAvg,
                    TimeToLevelSeconds = timeToLevelSecs,
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

    public readonly record struct InstanceData(
        string CharacterName,
        AreaInstance Area,
        long JoinExperience,
        int JoinKills,
        DateTime JoinTime);
}