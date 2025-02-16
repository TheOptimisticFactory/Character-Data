using System;
using System.Collections.Generic;
using System.IO;
using CharacterData.Structs;
using Newtonsoft.Json;
using static CharacterData.Main;

namespace CharacterData.Export;

public class JsonSnapshotExporter : ISnapshotExporter
{
    private static readonly JsonSerializerSettings SerializerSettings = new()
    {
        DefaultValueHandling = DefaultValueHandling.Ignore,
        NullValueHandling = NullValueHandling.Ignore,
        MissingMemberHandling = MissingMemberHandling.Ignore
    };

    public void Initialize()
    {
    }

    public void UpdateConnection()
    {
    }

    public void Dispose()
    {
    }

    public void ExportSnapshot(SnapshotData snapshot, string characterName)
    {
        if (snapshot == null || !Plugin.Settings.InstanceExportSettings.JsonSettings.Enabled)
            return;

        if (Plugin.Settings.InstanceExportSettings.JsonSettings.AppendToFileMode)
            ExportAppendMode(snapshot, characterName);
        else
            ExportSeparateFiles(snapshot, characterName);
    }

    private void ExportAppendMode(SnapshotData snapshot, string characterName)
    {
        var folderPath = Path.Combine(Plugin.ConfigDirectory, "Snapshots", characterName);
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);
        var filePath = Path.Combine(folderPath, "Snapshots.json");

        List<SnapshotData> snapshots;
        if (File.Exists(filePath))
            try
            {
                var existingJson = File.ReadAllText(filePath);
                snapshots = JsonConvert.DeserializeObject<List<SnapshotData>>(existingJson, SerializerSettings) ?? [];
            }
            catch (Exception ex)
            {
                Plugin.LogError($"Error reading JSON snapshot file: {ex.Message}", 5);
                snapshots = [];
            }
        else
            snapshots = [];

        snapshots.Add(snapshot);

        try
        {
            var json = JsonConvert.SerializeObject(snapshots, Formatting.None, SerializerSettings);
            File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            Plugin.LogError($"Error writing JSON snapshot file: {ex.Message}", 5);
        }
    }

    private void ExportSeparateFiles(SnapshotData snapshot, string characterName)
    {
        var folderPath = Path.Combine(Plugin.ConfigDirectory, "Snapshots", characterName, "data");
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var snapshotDateTime = DateTimeOffset.FromUnixTimeSeconds(snapshot.SnapshotTime).LocalDateTime;
        var fileName = $"Snapshot_{snapshotDateTime:yyyy-MM-dd_HH-mm-ss}.json";
        var fullPath = Path.Combine(folderPath, fileName);

        try
        {
            var json = JsonConvert.SerializeObject(snapshot, Formatting.None);
            File.WriteAllText(fullPath, json);
        }
        catch (Exception ex)
        {
            Plugin.LogError($"Error writing JSON snapshot file: {ex.Message}", 5);
        }
    }
}