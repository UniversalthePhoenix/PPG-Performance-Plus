using PPGPerformancePlus.Core;
using PPGPerformancePlus.Services;

namespace PPGPerformancePlus.Systems;

public sealed class PhysicsController : IModSystem
{
    private ModContext? _context;

    public string Name => nameof(PhysicsController);

    public void Initialize(ModContext context)
    {
        _context = context;
    }

    public void Update(TimeSpan deltaTime)
    {
        if (_context is null || !_context.Config.EnableAutoSleep)
        {
            return;
        }

        var tracker = _context.GetRequiredService<FrameTimeTracker>();
        if (tracker.AverageFrameMs >= _context.Config.SustainedFrameThresholdMs)
        {
            _context.Logger.Info("PhysicsController detected sustained load; auto-sleep candidate logic would run here.");
        }
    }

    public void Shutdown()
    {
    }
}
