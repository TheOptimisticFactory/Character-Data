using System;
using System.IO;
using CharacterData.Structs;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using static CharacterData.Main;

namespace CharacterData.Export;

public class SqLiteSnapshotExporter : ISnapshotExporter, IDisposable
{
    private static readonly JsonSerializerSettings SerializerSettings = new()
    {
        DefaultValueHandling = DefaultValueHandling.Ignore,
        NullValueHandling = NullValueHandling.Ignore,
        MissingMemberHandling = MissingMemberHandling.Ignore
    };

    private SqliteConnection _connection;
    private string _dbPath;

    public void Initialize()
    {
        if (Plugin.Settings.InstanceExportSettings.SqLiteSettings.Enabled)
        {
            UpdateConnection();
            EnsureDatabase();
        }
    }

    public void UpdateConnection()
    {
        _dbPath = Path.Combine(Plugin.ConfigDirectory, "Snapshots", "snapshots.db");
        Directory.CreateDirectory(Path.GetDirectoryName(_dbPath));

        _connection?.Dispose();
        _connection = new SqliteConnection($"Data Source={_dbPath}");
        _connection.Open();
    }

    public void ExportSnapshot(SnapshotData snapshot, string characterName)
    {
        if (snapshot == null || !Plugin.Settings.InstanceExportSettings.SqLiteSettings.Enabled)
            return;

        try
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO snapshots (character_name, snapshot_time, snapshot_data)
                VALUES (@character_name, @snapshot_time, @snapshot_data)";

            cmd.Parameters.AddWithValue("@character_name", characterName);
            cmd.Parameters.AddWithValue("@snapshot_time", snapshot.SnapshotTime);
            cmd.Parameters.AddWithValue("@snapshot_data", JsonConvert.SerializeObject(snapshot, SerializerSettings));

            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Plugin.LogError($"Error logging snapshot to SQLite: {ex.Message}", 10);
        }
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }

    private void EnsureDatabase()
    {
        try
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS snapshots (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    character_name TEXT,
                    snapshot_time INTEGER,
                    snapshot_data TEXT
                )";

            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Plugin.LogError($"Error initializing SQLite database: {ex.Message}", 10);
        }
    }
}