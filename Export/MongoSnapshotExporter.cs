using System;
using CharacterData.Structs;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using static CharacterData.Main;

namespace CharacterData.Export;

public class MongoSnapshotExporter : ISnapshotExporter, IDisposable
{
    private static readonly JsonSerializerSettings SerializerSettings = new()
    {
        DefaultValueHandling = DefaultValueHandling.Ignore,
        NullValueHandling = NullValueHandling.Ignore,
        MissingMemberHandling = MissingMemberHandling.Ignore
    };

    private string _currentConnectionString;
    private IMongoClient _mongoClient;

    public void Initialize()
    {
        if (Plugin.Settings.InstanceExportSettings.MongoSettings.Enabled) UpdateConnection();
    }

    public void UpdateConnection()
    {
        var newConnectionString = Plugin.Settings.InstanceExportSettings.MongoSettings.ConnectionString.Value;
        if (_currentConnectionString != newConnectionString)
        {
            _mongoClient?.Cluster?.Dispose();
            _mongoClient = new MongoClient(newConnectionString);
            _currentConnectionString = newConnectionString;
        }
    }

    public void ExportSnapshot(SnapshotData snapshot, string characterName)
    {
        if (snapshot == null || !Plugin.Settings.InstanceExportSettings.MongoSettings.Enabled)
            return;

        try
        {
            var database = _mongoClient.GetDatabase(Plugin.Settings.InstanceExportSettings.MongoSettings.Database);
            var collection = database.GetCollection<BsonDocument>(characterName);

            var snapshotJson = JsonConvert.SerializeObject(snapshot, Formatting.None, SerializerSettings);
            var snapshotDoc = BsonDocument.Parse(snapshotJson);

            collection.InsertOne(snapshotDoc);
        }
        catch (Exception ex)
        {
            Plugin.LogError($"Error logging snapshot to MongoDB: {ex.Message}", 10);
        }
    }

    public void Dispose()
    {
        _mongoClient?.Cluster?.Dispose();
    }
}