using System;
using CharacterData.Structs;
using Newtonsoft.Json;
using Npgsql;
using static CharacterData.Main;

namespace CharacterData.Export;

public class PostgresSnapshotExporter : ISnapshotExporter, IDisposable
{
    private string _currentConnectionString;
    private NpgsqlDataSource _dataSource;

    public void Initialize()
    {
        if (Plugin.Settings.InstanceExportSettings.PostgresSettings.Enabled)
        {
            UpdateConnection();
            EnsureDatabase();
        }
    }

    public void UpdateConnection()
    {
        var newConnectionString = Plugin.Settings.InstanceExportSettings.PostgresSettings.ConnectionString.Value;
        if (_currentConnectionString != newConnectionString)
        {
            _dataSource?.Dispose();
            _dataSource = NpgsqlDataSource.Create(newConnectionString);
            _currentConnectionString = newConnectionString;
        }
    }

    public void ExportSnapshot(SnapshotData snapshot, string characterName)
    {
        if (snapshot == null || !Plugin.Settings.InstanceExportSettings.PostgresSettings.Enabled)
            return;

        try
        {
            using var conn = _dataSource.CreateConnection();
            conn.Open();

            var snapshotJson = JsonConvert.SerializeObject(snapshot);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO snapshots (character_name, snapshot_time, snapshot_data)
                VALUES (@character_name, @snapshot_time, @snapshot_data::jsonb)";

            cmd.Parameters.AddWithValue("@character_name", characterName);
            cmd.Parameters.AddWithValue("@snapshot_time", snapshot.SnapshotTime);
            cmd.Parameters.AddWithValue("@snapshot_data", snapshotJson);

            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Plugin.LogError($"Error logging snapshot to PostgreSQL: {ex.Message}", 10);
        }
    }

    public void Dispose()
    {
        _dataSource?.Dispose();
    }

    private void EnsureDatabase()
    {
        try
        {
            using var conn = _dataSource.CreateConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS snapshots (
                    id SERIAL PRIMARY KEY,
                    character_name TEXT,
                    snapshot_time BIGINT,
                    snapshot_data JSONB
                )";

            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Plugin.LogError($"Error initializing PostgreSQL database: {ex.Message}", 10);
        }
    }
}