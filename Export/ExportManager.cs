using System;
using System.Collections.Generic;
using CharacterData.Structs;
using static CharacterData.Main;

namespace CharacterData.Export;

public class ExportManager : IDisposable
{
    private readonly List<ISnapshotExporter> _exporters =
    [
        new JsonSnapshotExporter(),
        new MongoSnapshotExporter(),
        new PostgresSnapshotExporter(),
        new SqLiteSnapshotExporter()
    ];

    public void Dispose()
    {
        foreach (var exporter in _exporters)
            if (exporter is IDisposable disposable)
                disposable.Dispose();
    }

    public void Initialize()
    {
        foreach (var exporter in _exporters) exporter.Initialize();

        Plugin.Settings.InstanceExportSettings.MongoSettings.ConnectionString.OnValueChanged += () =>
            GetExporter<MongoSnapshotExporter>()?.UpdateConnection();

        Plugin.Settings.InstanceExportSettings.PostgresSettings.ConnectionString.OnValueChanged += () =>
            GetExporter<PostgresSnapshotExporter>()?.UpdateConnection();
    }

    private T GetExporter<T>() where T : class, ISnapshotExporter
    {
        return _exporters.Find(e => e is T) as T;
    }

    public void ExportSnapshot(SnapshotData snapshot, string characterName)
    {
        foreach (var exporter in _exporters) exporter.ExportSnapshot(snapshot, characterName);
    }
}