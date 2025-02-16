using CharacterData.NewFolder;
using CharacterData.Render;
using ExileCore;
using MongoDB.Driver;

namespace CharacterData;

public class Main : BaseSettingsPlugin<Settings>
{
    public static Main Plugin;
    public static MongoClient MongoClient;

    public Main()
    {
        Name = "Character Data";
    }

    public override bool Initialise()
    {
        Plugin = this;
        MongoClient = new MongoClient(Settings.SnapshotSettings.MongoConnection);

        // Initialize plugin logic module
        PluginLogic.Initialise();

        // Listen for changes to the Mongo connection string
        Settings.SnapshotSettings.MongoConnection.OnValueChanged += () =>
        {
            MongoClient = new MongoClient(Settings.SnapshotSettings.MongoConnection);
        };

        return true;
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