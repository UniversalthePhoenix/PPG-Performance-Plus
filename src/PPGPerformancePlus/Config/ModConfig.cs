namespace PPGPerformancePlus.Config;

public sealed class ModConfig
{
    public bool FirstRun { get; set; } = true;
    public bool ShowNotifications { get; set; } = true;
    public bool SafeSpawnMode { get; set; } = true;
    public bool EnableAutoSleep { get; set; } = true;
    public bool EnableFreezeProtection { get; set; } = true;
    public bool EnableLagWarnings { get; set; } = true;
    public int TargetSpawnBatchSize { get; set; } = 25;
    public double HeavySpawnEntityThreshold { get; set; } = 150;
    public double FrameSpikeThresholdMs { get; set; } = 24d;
    public double SustainedFrameThresholdMs { get; set; } = 18d;
    public int ConsecutiveLagFramesForWarning { get; set; } = 6;
    public int AutoSleepIdleSeconds { get; set; } = 5;
    public int NotificationCooldownSeconds { get; set; } = 15;
    public string SettingsKeybind { get; set; } = "F10";
    public HashSet<string> IgnoredMods { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
