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

        if (Plugin?.GameController is { Player: not null, Area.CurrentArea: not null })
        {
            var player = Plugin.GameController.Player.GetComponent<Player>();
            if (player != null)
            {
                var currentKills = TryGetStat(GameStat.CharacterKillCount);
                long currentXp = player.XP;
                var currentGold = Plugin.GameController.IngameState.ServerData.Gold;

                _currentInstance = new InstanceData(
                    player.PlayerName, Plugin.GameController.Area.CurrentArea, currentXp, currentKills, currentGold, DateTime.Now);
            }
        }
    }

    public static void Update()
    {
        if (Plugin?.GameController?.Player == null)
        {
            WaitingForPlayer = true;
            return;
        }

        if (WaitingForPlayer || PendingAreaReset)
            CreateNewInstance();
        else
            CurrentSnapshot = CreateSnapshot();
    }

    public static void AreaChange(AreaInstance area)
    {
        if (!_initialised) return;

        if (ShouldLog())
            ExportCurrentSnapshot(area);

        ResetForNewArea();
    }

    private static void CreateNewInstance()
    {
        var player = Plugin.GameController.Player.GetComponent<Player>();
        if (player == null) return;

        var currentArea = Plugin.GameController.Area?.CurrentArea;
        if (currentArea == null) return;

        var currentKills = TryGetStat(GameStat.CharacterKillCount);
        long currentXp = player.XP;
        var currentGold = Plugin.GameController.IngameState.ServerData.Gold;

        _currentInstance = new InstanceData(player.PlayerName, currentArea, currentXp, currentKills, currentGold, DateTime.Now);

        if (WaitingForPlayer)
        {
            WaitingForPlayer = false;
            _initialised = true;
        }

        if (PendingAreaReset)
            PendingAreaReset = false;
    }

    private static void ExportCurrentSnapshot(AreaInstance newArea)
    {
        if (CurrentSnapshot?.Player == null) return;

        CurrentSnapshot.EndArea = new AreaData
        {
            Name = newArea.Name,
            Level = newArea.RealLevel,
            Act = newArea.Act,
            Difference = CurrentSnapshot.Player.Level - newArea.RealLevel
        };

        Plugin.ExportManager.ExportSnapshot(CurrentSnapshot, _currentInstance.CharacterName);

        if (Snapshots.Count >= 10)
            Snapshots.RemoveAt(0);
        Snapshots.Add(CurrentSnapshot);
    }

    private static void ResetForNewArea()
    {
        WaitingForPlayer = true;
        PendingAreaReset = true;
    }

    private static int TryGetStat(GameStat stat)
    {
        return Plugin?.GameController?.Player?.Stats?.GetValueOrDefault(stat, 0) ?? 0;
    }

    private static bool ShouldLog()
    {
        if (!Plugin.Settings.InstanceExportSettings.Enabled)
            return false;

        var currentPlayer = Plugin.GameController?.Player?.GetComponent<Player>();
        if (currentPlayer == null || _currentInstance.CharacterName != currentPlayer.PlayerName)
            return false;

        if (Plugin.Settings.InstanceExportSettings.DisableConditionalShouldLogChecks)
            return true;

        var currentKills = TryGetStat(GameStat.CharacterKillCount);
        var xpGained = currentPlayer.XP - _currentInstance.JoinExperience;
        var killsGained = currentKills - _currentInstance.JoinKills;
        var hasProgress = xpGained > 0 || killsGained > 0;

        if (!hasProgress && _currentInstance.Area.IsPeaceful && !Plugin.Settings.InstanceExportSettings.EnablePeacefulAreas)
            return false;

        return hasProgress;
    }

    private static SnapshotData CreateSnapshot()
    {
        var playerComp = Plugin.GameController.Player?.GetComponent<Player>();
        var lifeComp = Plugin.GameController.Player?.GetComponent<Life>();

        if (playerComp == null || lifeComp == null)
            return null;

        var currentKills = TryGetStat(GameStat.CharacterKillCount);
        var currentXp = playerComp.XP;
        var xpGained = currentXp - _currentInstance.JoinExperience;
        var timeElapsed = (DateTime.Now - _currentInstance.JoinTime).TotalSeconds;
        var currentGold = Plugin.GameController.IngameState.ServerData.Gold;
        var goldGained = currentGold - _currentInstance.JoinGold;
        var areaKills = currentKills - _currentInstance.JoinKills;

        var progressPct = CharacterUtils.CalculateProgress(playerComp.Level, playerComp.XP);
        var levelPercent = CharacterUtils.GetLevelGainPercent(playerComp.Level, xpGained);
        var xpPerHour = timeElapsed > 0 ? xpGained / timeElapsed * 3600 : 0.0;
        var xpPerMobAvg = areaKills > 0 ? (double)xpGained / areaKills : (double?)null;
        var runsToNext = CharacterUtils.GetRunsToNextLevel(playerComp.Level, playerComp.XP, xpGained);
        var totalRuns = CharacterUtils.GetTotalRuns(playerComp.Level, xpGained);
        var timeToLevelSecs = CharacterUtils.GetTimeToLevelSeconds(xpGained, timeElapsed, playerComp.Level, playerComp.XP);

        return new SnapshotData
        {
            SnapshotTime = DateTimeOffset.Now.ToUnixTimeSeconds(),
            AreaTimeSeconds = timeElapsed,
            Gold = new Gold { Start = _currentInstance.JoinGold, Gain = goldGained },
            StartArea = new AreaData
            {
                Name = _currentInstance.Area.Name,
                Level = _currentInstance.Area.RealLevel,
                Act = _currentInstance.Area.Act,
                Difference = playerComp.Level - _currentInstance.Area.RealLevel
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
                    TimeToLevelSeconds = timeToLevelSecs
                },
                Runs = new RunsData { RunsToNext = runsToNext, TotalRuns = totalRuns },
                Kills = new KillsData { Total = currentKills, Area = areaKills }
            },
            Resistances = CreateResistanceData(),
            Defenses = CreateDefenseData()
        };
    }

    private static ResistanceData CreateResistanceData()
    {
        return new ResistanceData
        {
            Fire = CreateResistanceDetail(GameStat.FireDamageResistancePct, GameStat.UncappedFireDamageResistancePct, GameStat.MaximumFireDamageResistancePct),
            Cold = CreateResistanceDetail(GameStat.ColdDamageResistancePct, GameStat.UncappedColdDamageResistancePct, GameStat.MaximumColdDamageResistancePct),
            Lightning = CreateResistanceDetail(
                GameStat.LightningDamageResistancePct, GameStat.UncappedLightningDamageResistancePct, GameStat.MaximumLightningDamageResistancePct),
            Chaos = CreateResistanceDetail(
                GameStat.ChaosDamageResistancePct, GameStat.UncappedChaosDamageResistancePct, GameStat.MaximumChaosDamageResistancePct)
        };
    }

    private static ResistanceDetail CreateResistanceDetail(GameStat cappedStat, GameStat uncappedStat, GameStat maxStat)
    {
        var capped = TryGetStat(cappedStat);
        var uncapped = TryGetStat(uncappedStat);
        var max = TryGetStat(maxStat) != 0 ? TryGetStat(maxStat) : 75;
        var diff = CharacterUtils.ResistanceDifference(capped, uncapped, max);

        return new ResistanceDetail { Capped = capped, Uncapped = uncapped, Max = max, Diff = diff };
    }

    private static DefenseData CreateDefenseData()
    {
        return new DefenseData
        {
            Armor = new ArmorData
            {
                Rating = TryGetStat(GameStat.PhysicalDamageReductionRating),
                DisplayReduction = TryGetStat(GameStat.DisplayEstimatedPhysicalDamageReducitonPct)
            },
            Evasion = new EvasionData
            {
                Rating = TryGetStat(GameStat.EvasionRating),
                ChanceToEvade = TryGetStat(GameStat.ChanceToEvadePct)
            },
            Block = new BlockData
            {
                AttackBlockPct = TryGetStat(GameStat.AttackBlockPct)
            }
        };
    }

    public readonly record struct InstanceData(string CharacterName, AreaInstance Area, long JoinExperience, int JoinKills, int JoinGold, DateTime JoinTime);
}