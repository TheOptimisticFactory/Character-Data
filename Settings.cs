using ExileCore.Shared.Attributes;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using SharpDX;

namespace CharacterData;

public static class ColorExtensions
{
    public static Color ToSharpDx(this System.Drawing.Color color)
    {
        return new Color(color.R, color.G, color.B, color.A);
    }
}

public class Settings : ISettings
{
    public ToggleNode DebugLog { get; set; } = new ToggleNode(false);
    public ExportSettings InstanceExportSettings { get; set; } = new();
    public LevelSettings LevelSettings { get; set; } = new();
    public ResistanceSettings ResistanceSettings { get; set; } = new();
    public DefenseSettings DefenseSettings { get; set; } = new();
    public BackgroundSettings BackgroundSettings { get; set; } = new();
    public ToggleNode Enable { get; set; } = new(false);
}

[Submenu]
public class ExportSettings
{
    public ToggleNode Enabled { get; set; } = new(true);
    [Menu("Enable logging Peaceful areas.", "This will log IsHideout and IsTown areas as well as anything else under IsPeaceful.")]
    public ToggleNode EnablePeacefulAreas { get; set; } = new(true);
    [Menu("Ignore Conditional Log Checks", "This will ignore logging regardless if we have increased kills or gained xp\nThis does not override logging Peaceful areas.")]
    public ToggleNode DisableConditionalShouldLogChecks { get; set; } = new(true);
    public JsonSettings JsonSettings { get; set; } = new();
    public MongoSettings MongoSettings { get; set; } = new();
    public PostgresSettings PostgresSettings { get; set; } = new();
    public SqLiteSettings SqLiteSettings { get; set; } = new();
}

[Submenu (CollapsedByDefault = false)]
public class JsonSettings
{
    [Menu("Enable and allow CharacterDataFrontend compatibility", "This allows the use of https://detectivesquirrel.github.io/CharacterDataFrontend/\nIf you know how to extract your data from the other exporters\nfeel free to do so.")]
    public ToggleNode Enabled { get; set; } = new(true);
    [Menu("Combine into Same File", "Enabled: Allows the use of https://detectivesquirrel.github.io/CharacterDataFrontend/ as its all logged into a single file\nDisabled: logged to individual files.")]
    public ToggleNode AppendToFileMode { get; set; } = new(true);
}

[Submenu(CollapsedByDefault = true)]
public class MongoSettings
{
    public ToggleNode Enabled { get; set; } = new(false);
    public TextNode ConnectionString { get; set; } = new("mongodb://localhost:27017");
    public TextNode Database { get; set; } = new("Snapshots");
}

[Submenu(CollapsedByDefault = true)]
public class PostgresSettings
{
    public ToggleNode Enabled { get; set; } = new(false);
    public TextNode ConnectionString { get; set; } = new("Host=localhost;Port=5432;Database=snapshots;Username=postgres;Password=password");
}

[Submenu(CollapsedByDefault = true)]
public class SqLiteSettings
{
    public ToggleNode Enabled { get; set; } = new(false);
}

[Submenu]
public class DefenseSettings
{
    public ToggleNode Enabled { get; set; } = new(true);

    public ColorNode ArmorColor { get; set; } =
        System.Drawing.Color.FromArgb(255, 200, 200, 200).ToSharpDx();

    public ColorNode EvasionColor { get; set; } =
        System.Drawing.Color.FromArgb(255, 150, 200, 150).ToSharpDx();

    public ColorNode BlockColor { get; set; } =
        System.Drawing.Color.FromArgb(255, 200, 150, 150).ToSharpDx();

    public RangeNode<int> DefenseX { get; set; } = new(1063, 0, 4000);

    public RangeNode<int> DefenseY { get; set; } = new(1286, 0, 4000);
}

[Submenu]
public class ResistanceSettings
{
    public ToggleNode Enabled { get; set; } = new(true);

    public ColorNode FireResistanceColor { get; set; } =
        System.Drawing.Color.FromArgb(255, 255, 85, 85).ToSharpDx();

    public ColorNode ColdResistanceColor { get; set; } =
        System.Drawing.Color.FromArgb(255, 77, 134, 255).ToSharpDx();

    public ColorNode LightningResistanceColor { get; set; } =
        System.Drawing.Color.FromArgb(255, 253, 245, 75).ToSharpDx();

    public ColorNode ChaosResistanceColor { get; set; } =
        System.Drawing.Color.FromArgb(255, 255, 91, 179).ToSharpDx();

    public RangeNode<int> ResistanceX { get; set; } = new(1063, 0, 4000);

    public RangeNode<int> ResistanceY { get; set; } = new(1236, 0, 4000);
}

[Submenu]
public class LevelSettings
{
    public ToggleNode Enabled { get; set; } = new(true);

    public ColorNode TextColor { get; set; } =
        System.Drawing.Color.White.ToSharpDx();

    public RangeNode<int> LevelPositionX { get; set; } = new(680, 0, 4000);

    public RangeNode<int> LevelPositionY { get; set; } = new(1236, 0, 4000);
}

[Submenu]
public class BackgroundSettings
{
    public ToggleNode Enabled { get; set; } = new(true);

    public ColorNode BackgroundColor { get; set; } =
        System.Drawing.Color.FromArgb(197, 0, 0, 0).ToSharpDx();

    public RangeNode<int> ResolutionLeft { get; internal set; } = new(667, 0, 4000);

    public RangeNode<int> ResolutionTop { get; internal set; } = new(1230, 0, 4000);

    public RangeNode<int> ResolutionRight { get; internal set; } = new(1256, 0, 4000);

    public RangeNode<int> ResolutionBottom { get; internal set; } = new(1349, 0, 4000);
}