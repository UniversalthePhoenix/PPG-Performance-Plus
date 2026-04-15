namespace PPGPerformancePlus.Services;

public sealed class ModProfile
{
    public required string SourceId { get; init; }
    public int SpawnCount { get; private set; }
    public int SpikeCount { get; private set; }
    public double WorstFrameTimeMs { get; private set; }

    public void RecordSpawn() => SpawnCount++;

    public void RecordSpike(double frameTimeMs)
    {
        SpikeCount++;
        WorstFrameTimeMs = Math.Max(WorstFrameTimeMs, frameTimeMs);
    }
}
