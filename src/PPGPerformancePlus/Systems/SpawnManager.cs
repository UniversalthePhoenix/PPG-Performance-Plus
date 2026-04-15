using PPGPerformancePlus.Config;
using PPGPerformancePlus.Core;
using PPGPerformancePlus.Integration;
using PPGPerformancePlus.Services;

namespace PPGPerformancePlus.Systems;

public sealed class SpawnManager : IModSystem
{
    private ModContext? _context;
    private SpawnQueue? _spawnQueue;

    public string Name => nameof(SpawnManager);

    public void Initialize(ModContext context)
    {
        _context = context;
        _spawnQueue = new SpawnQueue();
        context.RegisterService(_spawnQueue);
    }

    public void Update(TimeSpan deltaTime)
    {
        if (_context is null || _spawnQueue is null)
        {
            return;
        }

        var request = _spawnQueue.Current ?? _spawnQueue.DequeueNext();
        if (request is null)
        {
            return;
        }

        var batchSize = ResolveBatchSize(_context.Config, _context.GameBridge.CurrentFrameTimeMs);

        if (_context.Config.SafeSpawnMode && request.IsHeavy(_context.Config.HeavySpawnEntityThreshold))
        {
            _context.GameBridge.PauseSimulation();
        }

        request.SpawnBatchAction(batchSize);
        _context.GetRequiredService<ModProfiler>().RecordSpawn(request.SourceId);

        if (_context.Config.SafeSpawnMode && _context.GameBridge.IsSimulationPaused)
        {
            _context.GameBridge.ResumeSimulation();
        }

        _spawnQueue.DequeueNext();
    }

    public void Shutdown()
    {
        _spawnQueue?.Clear();
    }

    public void Enqueue(SpawnRequest request)
    {
        _spawnQueue?.Enqueue(request);
    }

    private static int ResolveBatchSize(ModConfig config, double frameTimeMs)
    {
        var batchSize = config.TargetSpawnBatchSize;

        if (frameTimeMs >= config.FrameSpikeThresholdMs)
        {
            batchSize /= 3;
        }
        else if (frameTimeMs >= config.SustainedFrameThresholdMs)
        {
            batchSize /= 2;
        }

        return Math.Max(1, batchSize);
    }
}
