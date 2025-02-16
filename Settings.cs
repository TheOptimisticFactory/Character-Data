using ExileCore.Shared.Attributes;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using SharpDX;

namespace CharacterData;

public static class ColorExtensions
{
    public static Color ToSharpDX(this System.Drawing.Color color)
    {
        return new Color(color.R, color.G, color.B, color.A);
    }
}

public class Settings : ISettings
{
    public SnapshotSettings SnapshotSettings { get; set; } = new();
    public LevelSettings LevelSettings { get; set; } = new();
    public ResistanceSettings ResistanceSettings { get; set; } = new();
    public DefenseSettings DefenseSettings { get; set; } = new();
    public BackgroundSettings BackgroundSettings { get; set; } = new();
    public ToggleNode Enable { get; set; } = new(false);
}

[Submenu]
public class SnapshotSettings
{
    public ToggleNode Enabled { get; set; } = new(true);

    public ToggleNode LogAllAreaChanges { get; set; } = new(false);

    public ToggleNode AppendToFileMode { get; set; } = new(true);

    public ToggleNode MongoEnabled { get; set; } = new(false);

    public TextNode MongoConnection { get; set; } = new("mongodb://localhost:27017");

    public TextNode MongoDatabase { get; set; } = new("Snapshots");
}

[Submenu]
public class DefenseSettings
{
    public ToggleNode Enabled { get; set; } = new(true);

    public ColorNode ArmorColor { get; set; } =
        System.Drawing.Color.FromArgb(255, 200, 200, 200).ToSharpDX();

    public ColorNode EvasionColor { get; set; } =
        System.Drawing.Color.FromArgb(255, 150, 200, 150).ToSharpDX();

    public ColorNode BlockColor { get; set; } =
        System.Drawing.Color.FromArgb(255, 200, 150, 150).ToSharpDX();

    public RangeNode<int> DefenseX { get; set; } = new(1063, 0, 2000);

    public RangeNode<int> DefenseY { get; set; } = new(1286, 0, 2000);
}

[Submenu]
public class ResistanceSettings
{
    public ToggleNode Enabled { get; set; } = new(true);

    public ColorNode FireResistanceColor { get; set; } =
        System.Drawing.Color.FromArgb(255, 255, 85, 85).ToSharpDX();

    public ColorNode ColdResistanceColor { get; set; } =
        System.Drawing.Color.FromArgb(255, 77, 134, 255).ToSharpDX();

    public ColorNode LightningResistanceColor { get; set; } =
        System.Drawing.Color.FromArgb(255, 253, 245, 75).ToSharpDX();

    public ColorNode ChaosResistanceColor { get; set; } =
        System.Drawing.Color.FromArgb(255, 255, 91, 179).ToSharpDX();

    public RangeNode<int> ResistanceX { get; set; } = new(1063, 0, 2000);

    public RangeNode<int> ResistanceY { get; set; } = new(1236, 0, 2000);
}

[Submenu]
public class LevelSettings
{
    public ToggleNode Enabled { get; set; } = new(true);

    public ColorNode TextColor { get; set; } =
        System.Drawing.Color.White.ToSharpDX();

    public RangeNode<int> LevelPositionX { get; set; } = new(680, 0, 2000);

    public RangeNode<int> LevelPositionY { get; set; } = new(1236, 0, 2000);
}

[Submenu]
public class BackgroundSettings
{
    public ToggleNode Enabled { get; set; } = new(true);

    public ColorNode BackgroundColor { get; set; } =
        System.Drawing.Color.FromArgb(197, 0, 0, 0).ToSharpDX();

    public RangeNode<int> ResolutionLeft { get; internal set; } = new(667, 0, 2000);

    public RangeNode<int> ResolutionTop { get; internal set; } = new(1230, 0, 2000);

    public RangeNode<int> ResolutionRight { get; internal set; } = new(1256, 0, 2000);

    public RangeNode<int> ResolutionBottom { get; internal set; } = new(1349, 0, 2000);
}