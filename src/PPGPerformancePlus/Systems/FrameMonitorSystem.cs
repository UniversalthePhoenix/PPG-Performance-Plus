using PPGPerformancePlus.Core;
using PPGPerformancePlus.Services;

namespace PPGPerformancePlus.Systems;

public sealed class FrameMonitorSystem : IModSystem
{
    private ModContext? _context;

    public string Name => nameof(FrameMonitorSystem);

    public void Initialize(ModContext context)
    {
        _context = context;
        context.RegisterService(new FrameTimeTracker());
        context.RegisterService(new LagHistory());
    }

    public void Update(TimeSpan deltaTime)
    {
        if (_context is null)
        {
            return;
        }

        var tracker = _context.GetRequiredService<FrameTimeTracker>();
        var frameTimeMs = _context.GameBridge.CurrentFrameTimeMs > 0
            ? _context.GameBridge.CurrentFrameTimeMs
            : deltaTime.TotalMilliseconds;

        tracker.Record(frameTimeMs);

        if (frameTimeMs >= _context.Config.FrameSpikeThresholdMs)
        {
            var profiler = _context.GetRequiredService<ModProfiler>();
            var source = profiler.GetMostLikelySource();
            if (!string.IsNullOrWhiteSpace(source))
            {
                profiler.RecordSpike(source, frameTimeMs);
            }
        }
    }

    public void Shutdown()
    {
    }
}
