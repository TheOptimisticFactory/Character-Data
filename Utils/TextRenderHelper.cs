using ImGuiNET;
using SharpDX;
using Graphics = ExileCore.Graphics;
using Vector2 = System.Numerics.Vector2;

namespace CharacterData.Utils;

public static class TextRenderHelper
{
    public static void DrawMultilineText(Graphics graphics, string[] lines, Vector2 position, Color[] colors)
    {
        var lineHeight = ImGui.GetTextLineHeight();
        for (var i = 0; i < lines.Length; i++)
        {
            graphics.DrawText(lines[i], position with {Y = position.Y + lineHeight * i}, colors[i]);
        }
    }

    public static void DrawMultilineText(Graphics graphics, string[] lines, Vector2 position, Color color)
    {
        var colors = new Color[lines.Length];
        for (var i = 0; i < lines.Length; i++)
        {
            colors[i] = color;
        }

        DrawMultilineText(graphics, lines, position, colors);
    }
}