using CharacterData.Utils;
using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.Shared.Enums;
using System;
using System.Collections.Generic;
using SharpDX;
using Vector2 = System.Numerics.Vector2;

namespace CharacterData;

public class CharacterData : BaseSettingsPlugin<CharacterDataSettings>
{
    private bool _initialised;
    private bool _pendingAreaReset;
    private bool _waitingForPlayer = true;
    private InstanceData currentInstance;

    public override bool Initialise()
    {
        _initialised = false;
        _waitingForPlayer = true;
        return true;
    }

    public override Job Tick()
    {
        if (_waitingForPlayer || _pendingAreaReset)
        {
            var player = GameController.Player?.GetComponent<Player>();
            if (player != null)
            {
                var currentKills = TryGetStat(GameStat.CharacterKillCount);
                var currentXP = player.XP;
                currentInstance = new InstanceData(currentXP, currentKills, DateTime.Now);

                if (_waitingForPlayer)
                {
                    _waitingForPlayer = false;
                    _initialised = true;
                }

                if (_pendingAreaReset)
                {
                    _pendingAreaReset = false;
                }
            }
        }

        return null;
    }

    public override void AreaChange(AreaInstance area)
    {
        if (_initialised)
        {
            _pendingAreaReset = true;
        }
    }

    public override void Render()
    {
        if (_waitingForPlayer || _pendingAreaReset) return;

        DrawBackground();
        DrawResistances();
        DrawDefenses();
        DrawExperienceData();
    }

    private void DrawBackground()
    {
        if (!Settings.BackgroundSettings.Enabled) return;
        Graphics.DrawBox(new RectangleF
        {
            Left = Settings.BackgroundSettings.ResolutionLeft,
            Top = Settings.BackgroundSettings.ResolutionTop,
            Right = Settings.BackgroundSettings.ResolutionRight,
            Bottom = Settings.BackgroundSettings.ResolutionBottom
        }, Settings.BackgroundSettings.BackgroundColor);
    }

    private void DrawResistances()
    {
        if (!Settings.ResistanceSettings.Enabled) return;

        var FireRes = TryGetStat(GameStat.FireDamageResistancePct);
        var FireResTotal = TryGetStat(GameStat.UncappedFireDamageResistancePct);
        var MaxFireRes = TryGetStat(GameStat.MaximumFireDamageResistancePct) != 0 ? TryGetStat(GameStat.MaximumFireDamageResistancePct) : 75;

        var ColdRes = TryGetStat(GameStat.ColdDamageResistancePct);
        var ColdResTotal = TryGetStat(GameStat.UncappedColdDamageResistancePct);
        var MaxColdRes = TryGetStat(GameStat.MaximumColdDamageResistancePct) != 0 ? TryGetStat(GameStat.MaximumColdDamageResistancePct) : 75;

        var LightRes = TryGetStat(GameStat.LightningDamageResistancePct);
        var LightResTotal = TryGetStat(GameStat.UncappedLightningDamageResistancePct);
        var MaxLightRes = TryGetStat(GameStat.MaximumLightningDamageResistancePct) != 0 ? TryGetStat(GameStat.MaximumLightningDamageResistancePct) : 75;

        var ChaosRes = TryGetStat(GameStat.ChaosDamageResistancePct);
        var ChaosResTotal = TryGetStat(GameStat.UncappedChaosDamageResistancePct);
        var MaxChaosRes = TryGetStat(GameStat.MaximumChaosDamageResistancePct) != 0 ? TryGetStat(GameStat.MaximumChaosDamageResistancePct) : 75;

        var fireDiff = CharacterUtils.ResistanceDifference(FireRes, FireResTotal, MaxFireRes);
        var coldDiff = CharacterUtils.ResistanceDifference(ColdRes, ColdResTotal, MaxColdRes);
        var lightDiff = CharacterUtils.ResistanceDifference(LightRes, LightResTotal, MaxLightRes);
        var chaosDiff = CharacterUtils.ResistanceDifference(ChaosRes, ChaosResTotal, MaxChaosRes);

        var lines = new[]
        {
            $"{"Fire:",-8}{FireRes,-5}({(fireDiff > 0 ? "+" + fireDiff : fireDiff.ToString())})",
            $"{"Cold:",-8}{ColdRes,-5}({(coldDiff > 0 ? "+" + coldDiff : coldDiff.ToString())})",
            $"{"Light:",-8}{LightRes,-5}({(lightDiff > 0 ? "+" + lightDiff : lightDiff.ToString())})",
            $"{"Chaos:",-8}{ChaosRes,-5}({(chaosDiff > 0 ? "+" + chaosDiff : chaosDiff.ToString())})"
        };

        var colors = new[]
        {
            Settings.ResistanceSettings.FireResistanceColor.Value,
            Settings.ResistanceSettings.ColdResistanceColor.Value,
            Settings.ResistanceSettings.LightningResistanceColor.Value,
            Settings.ResistanceSettings.ChaosResistanceColor.Value
        };

        TextRenderHelper.DrawMultilineText(Graphics, lines, new Vector2(Settings.ResistanceSettings.ResistanceX, Settings.ResistanceSettings.ResistanceY), colors);
    }

    private void DrawDefenses()
    {
        if (!Settings.DefenseSettings.Enabled) return;

        var armor = TryGetStat(GameStat.PhysicalDamageReductionRating);
        var physReduction = TryGetStat(GameStat.DisplayEstimatedPhysicalDamageReducitonPct);

        var evasion = TryGetStat(GameStat.EvasionRating);
        var evadeChance = TryGetStat(GameStat.ChanceToEvadePct);

        var blockChance = TryGetStat(GameStat.AttackBlockPct);

        var lines = new[]
        {
            $"{"Armor:",-8}{physReduction + "%",-5}({armor:#,##0})",
            $"{"Evade:",-8}{evadeChance + "%",-5}({evasion:#,##0})",
            $"{"Block:",-8}{blockChance + "%"}"
        };

        var colors = new[]
        {
            Settings.DefenseSettings.ArmorColor.Value,
            Settings.DefenseSettings.EvasionColor.Value,
            Settings.DefenseSettings.BlockColor.Value
        };

        TextRenderHelper.DrawMultilineText(Graphics, lines, new Vector2(Settings.DefenseSettings.DefenseX, Settings.DefenseSettings.DefenseY), colors);
    }

    private void DrawExperienceData()
    {
        if (!Settings.LevelSettings.Enabled) return;
        var player = GameController.Player?.GetComponent<Player>();
        if (player == null) return;

        var currentKills = TryGetStat(GameStat.CharacterKillCount);
        var progress = CharacterUtils.CalculateProgress(player.Level, player.XP);
        var expGained = player.XP - currentInstance.JoinExperience;
        var levelPercent = CharacterUtils.CalculateLevelPercent(player.Level, expGained);
        var runsToNext = CharacterUtils.CalculateRunsToNext(player.Level, player.XP, expGained);
        var totalRuns = CharacterUtils.CalculateTotalRuns(player.Level, expGained);
        var areaLevel = GameController.Game.IngameState.Data.CurrentAreaLevel;
        var levelDiff = player.Level - areaLevel;
        var levelDiffStr = levelDiff > 0 ? $"+{levelDiff}" : levelDiff.ToString();
        var timeElapsed = (DateTime.Now - currentInstance.JoinTime).TotalSeconds;
        var timeStr = CharacterUtils.FormatElapsedTime(timeElapsed);
        var xpPerHour = timeElapsed > 0 ? expGained / timeElapsed * 3600 : 0;
        var timeToLevel = CharacterUtils.CalculateTimeToLevel(expGained, timeElapsed, player.Level, player.XP);

        var areaKills = currentKills - currentInstance.JoinKills;

        var lines = new[]
        {
            $"Level: {player.Level} ({progress:N2}%)",
            $"Gained XP: {expGained:#,##0} ({levelPercent}%)",
            $"XP/hr: {xpPerHour:#,##0}",
            $"Time to Level: {timeToLevel}",
            $"Areas ETA: {runsToNext}/{totalRuns}",
            $"Area Dif: {levelDiffStr}",
            $"Kills: {currentKills:#,##0} (+{areaKills:#,##0})",
            $"Area Time: {timeStr}"
        };

        TextRenderHelper.DrawMultilineText(Graphics, lines, new Vector2(Settings.LevelSettings.LevelPositionX, Settings.LevelSettings.LevelPositionY), Settings.LevelSettings.TextColor);
    }

    private int TryGetStat(GameStat stat) => GameController.Player.Stats.GetValueOrDefault(stat, 0);

    private record struct InstanceData(long JoinExperience, int JoinKills, DateTime JoinTime);
}