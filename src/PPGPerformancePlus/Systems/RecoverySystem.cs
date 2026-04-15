using PPGPerformancePlus.Core;
using PPGPerformancePlus.Integration;
using PPGPerformancePlus.Services;

namespace PPGPerformancePlus.Systems;

public sealed class RecoverySystem : IModSystem
{
    private ModContext? _context;

    public string Name => nameof(RecoverySystem);

    public void Initialize(ModContext context)
    {
        _context = context;
    }

    public void Update(TimeSpan deltaTime)
    {
        if (_context is null || !_context.Config.EnableFreezeProtection)
        {
            return;
        }

        var tracker = _context.GetRequiredService<FrameTimeTracker>();
        if (tracker.LatestFrameMs < _context.Config.FrameSpikeThresholdMs * 2)
        {
            return;
        }

        _context.Logger.Warn("RecoverySystem triggered emergency freeze protection.");
        _context.GameBridge.PauseSimulation();
        _context.GetRequiredService<SpawnQueue>().Clear();
        _context.GameBridge.ShowNotification("Emergency recovery", "Spawn queue was cleared after severe frame stall detection.", NotificationSeverity.Critical);
        _context.GameBridge.ResumeSimulation();
    }

    public void Shutdown()
    {
    }
}
