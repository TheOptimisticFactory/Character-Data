namespace CharacterData.Structs;

public class SnapshotData
{
    public long SnapshotTime { get; set; }
    public double AreaTimeSeconds { get; set; }
    public Gold Gold { get; set; }
    public AreaData StartArea { get; set; }
    public AreaData EndArea { get; set; }
    public PlayerData Player { get; set; }
    public ResistanceData Resistances { get; set; }
    public DefenseData Defenses { get; set; }
}

public class Gold
{
    public int Start { get; set; }
    public int Gain { get; set; }
}

public class AreaData
{
    public string Name { get; set; }
    public int Level { get; set; }
    public int Act { get; set; }
    public int Difference { get; set; }
}

public class PlayerData
{
    public int Level { get; set; }
    public long Xp { get; set; }
    public int MaxHP { get; set; }
    public int MaxES { get; set; }
    public int MaxMana { get; set; }
    public XpData XpData { get; set; }
    public RunsData Runs { get; set; }
    public KillsData Kills { get; set; }
}

public class XpData
{
    public double ProgressPercent { get; set; }
    public long XpGained { get; set; }
    public double LevelPercent { get; set; }
    public double XpPerHour { get; set; }
    public double? XpPerMobAvg { get; set; }
    public double? TimeToLevelSeconds { get; set; }
}

public class RunsData
{
    public int? RunsToNext { get; set; }
    public int? TotalRuns { get; set; }
}

public class KillsData
{
    public int Total { get; set; }
    public int Area { get; set; }
}

public class ResistanceData
{
    public ResistanceDetail Fire { get; set; }
    public ResistanceDetail Cold { get; set; }
    public ResistanceDetail Lightning { get; set; }
    public ResistanceDetail Chaos { get; set; }
}

public class ResistanceDetail
{
    public int Capped { get; set; }
    public int Uncapped { get; set; }
    public int Max { get; set; }
    public int Diff { get; set; }
}

public class DefenseData
{
    public ArmorData Armor { get; set; }
    public EvasionData Evasion { get; set; }
    public BlockData Block { get; set; }
}

public class ArmorData
{
    public int Rating { get; set; }
    public int DisplayReduction { get; set; }
}

public class EvasionData
{
    public int Rating { get; set; }
    public int ChanceToEvade { get; set; }
}

public class BlockData
{
    public int AttackBlockPct { get; set; }
}