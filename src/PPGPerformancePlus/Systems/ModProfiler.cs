using PPGPerformancePlus.Core;
using PPGPerformancePlus.Services;

namespace PPGPerformancePlus.Systems;

public sealed class ModProfiler : IModSystem
{
    private readonly Dictionary<string, ModProfile> _profiles = new(StringComparer.OrdinalIgnoreCase);
    private string? _lastSpawnSource;

    public string Name => nameof(ModProfiler);

    public void Initialize(ModContext context)
    {
        context.RegisterService(this);
    }

    public void Update(TimeSpan deltaTime)
    {
    }

    public void Shutdown()
    {
    }

    public void RecordSpawn(string sourceId)
    {
        _lastSpawnSource = sourceId;
        GetOrCreate(sourceId).RecordSpawn();
    }

    public void RecordSpike(string sourceId, double frameTimeMs)
    {
        GetOrCreate(sourceId).RecordSpike(frameTimeMs);
    }

    public string? GetMostLikelySource()
    {
        if (_lastSpawnSource is not null)
        {
            return _lastSpawnSource;
        }

        return _profiles
            .OrderByDescending(pair => pair.Value.SpikeCount)
            .ThenByDescending(pair => pair.Value.WorstFrameTimeMs)
            .Select(pair => pair.Key)
            .FirstOrDefault();
    }

    private ModProfile GetOrCreate(string sourceId)
    {
        if (!_profiles.TryGetValue(sourceId, out var profile))
        {
            profile = new ModProfile { SourceId = sourceId };
            _profiles[sourceId] = profile;
        }

        return profile;
    }
}
