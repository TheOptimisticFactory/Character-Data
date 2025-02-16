using CharacterData.Export;
using CharacterData.Logic;
using CharacterData.Render;
using ExileCore;
using SQLitePCL;

namespace CharacterData;

public class Main : BaseSettingsPlugin<Settings>
{
    public static Main Plugin;
    public ExportManager ExportManager;

    public Main()
    {
        Name = "Character Data";
    }

    public override bool Initialise()
    {
        Plugin = this;

        Batteries_V2.Init();

        ExportManager = new ExportManager();
        ExportManager.Initialize();

        PluginLogic.Initialise();

        return true;
    }

    public override void OnPluginDestroyForHotReload()
    {
        base.OnPluginDestroyForHotReload();
        ExportManager?.Dispose();
    }

    public override void Dispose()
    {
        base.Dispose();
        ExportManager?.Dispose();
    }

    public override Job Tick()
    {
        PluginLogic.Update();
        return null;
    }

    public override void AreaChange(AreaInstance area)
    {
        PluginLogic.AreaChange(area);
    }

    public override void Render()
    {
        PluginRenderer.Render();
    }
}