using System.Linq;
using CharacterData.Logic;
using CharacterData.Structs;
using CharacterData.Utils;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using SharpDX;
using Vector2 = System.Numerics.Vector2;

namespace CharacterData.Render;

public static class PluginRenderer
{
    public static void Render()
    {
        if (PluginLogic.WaitingForPlayer || PluginLogic.PendingAreaReset)
            return;

        var ingameUi = Main.Plugin.GameController.Game.IngameState.IngameUi;
        if (!Main.Plugin.Settings.RenderSettings.IgnoreFullscreenPanels && ingameUi.FullscreenPanels.Any(x => x.IsVisible))
            return;

        if (!Main.Plugin.Settings.RenderSettings.IgnoreLargePanels && ingameUi.LargePanels.Any(x => x.IsVisible))
            return;

        if (!Main.Plugin.Settings.RenderSettings.IgnoreChatPanel && ingameUi.ChatTitlePanel.IsVisible)
            return;

        if (!Main.Plugin.Settings.RenderSettings.IgnoreLeftPanel && ingameUi.OpenLeftPanel.IsVisible)
            return;

        if (!Main.Plugin.Settings.RenderSettings.IgnoreRightPanel && ingameUi.OpenRightPanel.IsVisible)
            return;

        DrawBackground();
        DrawResistances();
        DrawDefenses();
        DrawExperienceData();
        DrawGold();
    }

    private static void DrawBackground()
    {
        if (!Main.Plugin.Settings.BackgroundSettings.Enabled)
            return;

        var rect = new RectangleF
        {
            Left = Main.Plugin.Settings.BackgroundSettings.ResolutionLeft,
            Top = Main.Plugin.Settings.BackgroundSettings.ResolutionTop,
            Right = Main.Plugin.Settings.BackgroundSettings.ResolutionRight,
            Bottom = Main.Plugin.Settings.BackgroundSettings.ResolutionBottom
        };

        Main.Plugin.Graphics.DrawBox(rect, Main.Plugin.Settings.BackgroundSettings.BackgroundColor);
    }

    private static void DrawResistances()
    {
        if (!Main.Plugin.Settings.ResistanceSettings.Enabled || PluginLogic.CurrentSnapshot == null)
            return;

        var snapshot = PluginLogic.CurrentSnapshot;

        var lines = new[]
        {
            $"{"Fire:",-8}{snapshot.Resistances.Fire.Capped,-5}({snapshot.Resistances.Fire.Diff:+0;-0;0})",
            $"{"Cold:",-8}{snapshot.Resistances.Cold.Capped,-5}({snapshot.Resistances.Cold.Diff:+0;-0;0})",
            $"{"Light:",-8}{snapshot.Resistances.Lightning.Capped,-5}({snapshot.Resistances.Lightning.Diff:+0;-0;0})",
            $"{"Chaos:",-8}{snapshot.Resistances.Chaos.Capped,-5}({snapshot.Resistances.Chaos.Diff:+0;-0;0})"
        };

        var colors = new[]
        {
            Main.Plugin.Settings.ResistanceSettings.FireResistanceColor.Value,
            Main.Plugin.Settings.ResistanceSettings.ColdResistanceColor.Value,
            Main.Plugin.Settings.ResistanceSettings.LightningResistanceColor.Value,
            Main.Plugin.Settings.ResistanceSettings.ChaosResistanceColor.Value
        };

        TextRenderHelper.DrawMultilineText(
            Main.Plugin.Graphics, lines, new Vector2(Main.Plugin.Settings.ResistanceSettings.ResistanceX, Main.Plugin.Settings.ResistanceSettings.ResistanceY),
            colors);
    }

    private static void DrawDefenses()
    {
        if (!Main.Plugin.Settings.DefenseSettings.Enabled || PluginLogic.CurrentSnapshot == null)
            return;

        var snapshot = PluginLogic.CurrentSnapshot;

        var lines = new[]
        {
            $"{"Armor:",-8}{snapshot.Defenses.Armor.DisplayReduction + "%",-5}" + $"({snapshot.Defenses.Armor.Rating:#,##0})",
            $"{"Evade:",-8}{snapshot.Defenses.Evasion.ChanceToEvade + "%",-5}" + $"({snapshot.Defenses.Evasion.Rating:#,##0})",
            $"{"Block:",-8}{snapshot.Defenses.Block.AttackBlockPct + "%"}"
        };

        var colors = new[]
        {
            Main.Plugin.Settings.DefenseSettings.ArmorColor.Value,
            Main.Plugin.Settings.DefenseSettings.EvasionColor.Value,
            Main.Plugin.Settings.DefenseSettings.BlockColor.Value
        };

        TextRenderHelper.DrawMultilineText(
            Main.Plugin.Graphics, lines, new Vector2(Main.Plugin.Settings.DefenseSettings.DefenseX, Main.Plugin.Settings.DefenseSettings.DefenseY), colors);
    }


    private static void DrawGold()
    {
        if (!Main.Plugin.Settings.GoldSettings.Enabled || PluginLogic.CurrentSnapshot == null)
            return;

        var snapshot = PluginLogic.CurrentSnapshot;

        var lines = new[]
        {
            $"Gold: {snapshot.Gold.Current:#,##0} ({snapshot.Gold.Gain:+#,##0;-#,##0;0})"
        };

        var colors = new[]
        {
            Main.Plugin.Settings.GoldSettings.GoldColor.Value
        };

        TextRenderHelper.DrawMultilineText(
            Main.Plugin.Graphics, lines, new Vector2(Main.Plugin.Settings.GoldSettings.GoldX, Main.Plugin.Settings.GoldSettings.GoldY), colors);
    }

    private static void DrawExperienceData()
    {
        if (!Main.Plugin.Settings.LevelSettings.Enabled || PluginLogic.CurrentSnapshot == null)
            return;

        var snapshot = PluginLogic.CurrentSnapshot;
        var xpData = snapshot.Player.XpData;
        var runs = snapshot.Player.Runs;
        var kills = snapshot.Player.Kills;

        var progress = xpData.ProgressPercent;
        var xpGained = xpData.XpGained;
        var levelPercent = xpData.LevelPercent;
        var xpPerHour = xpData.XpPerHour;

        var areaTimeDisplay = snapshot.AreaTimeSeconds > 0 ? CharacterUtils.FormatTime(snapshot.AreaTimeSeconds) : "-";
        var timeToLevelDisplay = xpData.TimeToLevelSeconds.HasValue ? CharacterUtils.FormatTime(xpData.TimeToLevelSeconds.Value) : "-";

        var runsToNextDisplay = runs.RunsToNext.HasValue ? runs.RunsToNext.Value.ToString("N0") : "-";
        var totalRunsDisplay = runs.TotalRuns.HasValue ? runs.TotalRuns.Value.ToString("N0") : "-";

        var player = Main.Plugin.GameController.Player?.GetComponent<Player>();
        if (player == null)
            return;

        var areaLevel = Main.Plugin.GameController.Game.IngameState.Data.CurrentAreaLevel;
        var levelDiff = player.Level - areaLevel;

        var lines = new[]
        {
            $"Level: {player.Level} ({progress:N2}%)",
            $"Gained XP: {xpGained:#,##0} ({levelPercent:N2}%)",
            $"XP/hr: {xpPerHour:#,##0}",
            $"Time to Level: {timeToLevelDisplay}",
            $"Areas ETA: {runsToNextDisplay}/{totalRunsDisplay}",
            $"Area Dif: {levelDiff:+#;-#;0}",
            $"Kills: {kills.Total:#,##0} ({kills.Area:+#,##0;-#,##0;0})",
            $"Area Time: {areaTimeDisplay}"
        };

        TextRenderHelper.DrawMultilineText(
            Main.Plugin.Graphics, lines, new Vector2(Main.Plugin.Settings.LevelSettings.LevelPositionX, Main.Plugin.Settings.LevelSettings.LevelPositionY),
            Main.Plugin.Settings.LevelSettings.TextColor);
    }
}