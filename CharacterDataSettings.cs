using System.Drawing;
using ExileCore.Shared.Attributes;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;

namespace CharacterData;

public static class ColorExtensions
{
    public static SharpDX.Color ToSharpDX(this Color color)
    {
        return new SharpDX.Color(color.R, color.G, color.B, color.A);
    }
}
public class CharacterDataSettings : ISettings
{
    public LevelSettings LevelSettings { get; set; } = new LevelSettings();
    public ResistanceSettings ResistanceSettings { get; set; } = new ResistanceSettings();
    public DefenseSettings DefenseSettings { get; set; } = new DefenseSettings();
    public BackgroundSettings BackgroundSettings { get; set; } = new BackgroundSettings();
    public ToggleNode Enable { get; set; } = new ToggleNode(false);
}

[Submenu]
public class DefenseSettings
{
    public ToggleNode Enabled { get; set; } = new ToggleNode(true);
    public ColorNode ArmorColor { get; set; } = Color.FromArgb(255, 200, 200, 200).ToSharpDX();
    public ColorNode EvasionColor { get; set; } = Color.FromArgb(255, 150, 200, 150).ToSharpDX();
    public ColorNode BlockColor { get; set; } = Color.FromArgb(255, 200, 150, 150).ToSharpDX();
    public RangeNode<int> DefenseX { get; set; } = new RangeNode<int>(1063, 0, 2000);
    public RangeNode<int> DefenseY { get; set; } = new RangeNode<int>(1286, 0, 2000);
}

[Submenu]
public class ResistanceSettings
{
    public ToggleNode Enabled { get; set; } = new ToggleNode(true);
    public ColorNode FireResistanceColor { get; set; } = Color.FromArgb(255, 255, 85, 85).ToSharpDX();
    public ColorNode ColdResistanceColor { get; set; } = Color.FromArgb(255, 77, 134, 255).ToSharpDX();
    public ColorNode LightningResistanceColor { get; set; } = Color.FromArgb(255, 253, 245, 75).ToSharpDX();
    public ColorNode ChaosResistanceColor { get; set; } = Color.FromArgb(255, 255, 91, 179).ToSharpDX();
    public RangeNode<int> ResistanceX { get; set; } = new RangeNode<int>(1063, 0, 2000);
    public RangeNode<int> ResistanceY { get; set; } = new RangeNode<int>(1236, 0, 2000);
}

[Submenu]
public class LevelSettings
{
    public ToggleNode Enabled { get; set; } = new ToggleNode(true);
    public ColorNode TextColor { get; set; } = Color.White.ToSharpDX();
    public RangeNode<int> LevelPositionX { get; set; } = new RangeNode<int>(680, 0, 2000);
    public RangeNode<int> LevelPositionY { get; set; } = new RangeNode<int>(1236, 0, 2000);
}

[Submenu]
public class BackgroundSettings
{
    public ToggleNode Enabled { get; set; } = new ToggleNode(true);
    public ColorNode BackgroundColor { get; set; } = Color.FromArgb(197, 0, 0, 0).ToSharpDX();
    public RangeNode<int> ResolutionLeft { get; internal set; } = new RangeNode<int>(667, 0, 2000);
    public RangeNode<int> ResolutionTop { get; internal set; } = new RangeNode<int>(1230, 0, 2000);
    public RangeNode<int> ResolutionRight { get; internal set; } = new RangeNode<int>(1256, 0, 2000);
    public RangeNode<int> ResolutionBottom { get; internal set; } = new RangeNode<int>(1349, 0, 2000);
}