using System;
using System.Collections.Generic;
using System.IO;
using CharacterData.Structs;
using ExileCore.PoEMemory.Components;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace CharacterData.Export;

public static class SnapshotExporter
{
    public static void ExportSnapshot(SnapshotData snapshot)
    {
        if (snapshot == null)
            return;

        var player = Main.Plugin.GameController.Player
            ?.GetComponent<Player>();
        var characterName = player != null && !string.IsNullOrWhiteSpace(player.PlayerName)
            ? player.PlayerName
            : "Unknown";

        if (Main.Plugin.Settings.SnapshotSettings.AppendToFileMode)
        {
            var folderPath =
                Path.Combine(Main.Plugin.ConfigDirectory, "Snapshots", characterName);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            var filePath = Path.Combine(folderPath, "Snapshots.json");

            List<SnapshotData> snapshots;
            if (File.Exists(filePath))
                try
                {
                    var existingJson = File.ReadAllText(filePath);
                    snapshots =
                        JsonConvert.DeserializeObject<List<SnapshotData>>(
                            existingJson) ??
                        new List<SnapshotData>();
                }
                catch (Exception ex)
                {
                    Main.Plugin.LogMessage("Error reading snapshot file: " + ex.Message, 5);
                    snapshots = new List<SnapshotData>();
                }
            else
                snapshots = new List<SnapshotData>();

            snapshots.Add(snapshot);

            try
            {
                var json = JsonConvert.SerializeObject(snapshots, Formatting.None);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Main.Plugin.LogMessage("Error writing snapshot file: " + ex.Message, 5);
            }
        }
        else
        {
            var folderPath = Path.Combine(
                Main.Plugin.ConfigDirectory, "Snapshots", characterName, "data");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var snapshotDateTime = DateTimeOffset.FromUnixTimeSeconds(snapshot.SnapshotTime)
                .LocalDateTime;
            var fileName = $"Snapshot_{snapshotDateTime:yyyy-MM-dd_HH-mm-ss}.json";

            var fullPath = Path.Combine(folderPath, fileName);
            var json = JsonConvert.SerializeObject(snapshot, Formatting.None);

            try
            {
                File.WriteAllText(fullPath, json);
            }
            catch (Exception ex)
            {
                Main.Plugin.LogMessage("Error writing snapshot file: " + ex.Message, 5);
            }
        }

        if (Main.Plugin.Settings.SnapshotSettings.MongoEnabled)
            LogSnapshotToMongo(snapshot, characterName);
    }

    private static void LogSnapshotToMongo(SnapshotData snapshot, string characterName)
    {
        try
        {
            var mongoDatabase = Main.Plugin.Settings.SnapshotSettings.MongoDatabase;
            var db = Main.MongoClient.GetDatabase(mongoDatabase);
            var collection = db.GetCollection<BsonDocument>(characterName);
            var snapshotJson = JsonConvert.SerializeObject(snapshot, Formatting.None);
            var snapshotDoc = BsonDocument.Parse(snapshotJson);

            collection.InsertOne(snapshotDoc);
        }
        catch (Exception ex)
        {
            Main.Plugin.LogError("Error logging snapshot to MongoDB: " + ex, 10);
        }
    }
}