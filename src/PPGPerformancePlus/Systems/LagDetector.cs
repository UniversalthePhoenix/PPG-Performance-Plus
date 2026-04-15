using PPGPerformancePlus.Core;
using PPGPerformancePlus.Integration;
using PPGPerformancePlus.Services;

namespace PPGPerformancePlus.Systems;

public sealed class LagDetector : IModSystem
{
    private ModContext? _context;
    private int _consecutiveLagFrames;
    private DateTimeOffset _lastWarningUtc = DateTimeOffset.MinValue;

    public string Name => nameof(LagDetector);

    public void Initialize(ModContext context)
    {
        _context = context;
    }

    public void Update(TimeSpan deltaTime)
    {
        if (_context is null || !_context.Config.EnableLagWarnings)
        {
            return;
        }

        var tracker = _context.GetRequiredService<FrameTimeTracker>();
        var frameTimeMs = tracker.LatestFrameMs;
        if (frameTimeMs >= _context.Config.SustainedFrameThresholdMs)
        {
            _consecutiveLagFrames++;
        }
        else
        {
            _consecutiveLagFrames = 0;
        }

        if (_consecutiveLagFrames < _context.Config.ConsecutiveLagFramesForWarning)
        {
            return;
        }

        var cooldown = TimeSpan.FromSeconds(_context.Config.NotificationCooldownSeconds);
        if (DateTimeOffset.UtcNow - _lastWarningUtc < cooldown)
        {
            return;
        }

        _lastWarningUtc = DateTimeOffset.UtcNow;
        var source = _context.GetRequiredService<ModProfiler>().GetMostLikelySource() ?? "Unknown source";
        _context.GetRequiredService<LagHistory>().Add(new LagIncident(DateTimeOffset.UtcNow, source, frameTimeMs, "Sustained frame degradation"));

        if (_context.Config.ShowNotifications && !_context.Config.IgnoredMods.Contains(source))
        {
            _context.GameBridge.ShowNotification("Performance warning", $"{source} is correlated with frame spikes.", NotificationSeverity.Warning);
        }
    }

    public void Shutdown()
    {
    }
}
