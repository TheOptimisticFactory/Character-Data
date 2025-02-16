using CharacterData.Structs;

namespace CharacterData.Export;

public interface ISnapshotExporter
{
    void ExportSnapshot(SnapshotData snapshot, string characterName);
    void Initialize();
    void UpdateConnection();
    void Dispose();
}