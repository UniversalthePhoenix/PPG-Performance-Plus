namespace PPGPerformancePlusMod
{
    [System.Serializable]
    public class ModConfig
    {
        public bool FirstRun = true;
        public bool ShowNotifications = true;
        public bool EnableLagWarnings = true;
        public bool EnableAutoSleep = true;
        public bool EnableDebrisControl = true;
        public bool EnableOffscreenOptimization = true;
        public bool EnableAdaptivePhysics = true;
        public bool EnableFreezeProtection = true;
        public bool EnableSafeSpawnStabilisation = true;
        public bool RemoveTinyDebris = false;

        public int ScanIntervalFrames = 20;
        public int NotificationCooldownSeconds = 15;
        public int ConsecutiveLagFramesForWarning = 8;
        public int HeavySpawnBodyThreshold = 80;
        public int DangerousSpawnBodyThreshold = 140;
        public int MaxDebrisBodies = 250;

        public float FrameSpikeThresholdMs = 24f;
        public float SustainedFrameThresholdMs = 18f;
        public float SevereFrameThresholdMs = 45f;
        public float IdleVelocityThreshold = 0.06f;
        public float AutoSleepDelaySeconds = 5f;
        public float SmallBodySizeThreshold = 0.45f;
        public float OffscreenSleepDelaySeconds = 8f;
        public float EmergencyModeSeconds = 6f;

        public int NormalVelocityIterations = 8;
        public int NormalPositionIterations = 3;
        public int ReducedVelocityIterations = 6;
        public int ReducedPositionIterations = 2;
        public int EmergencyVelocityIterations = 4;
        public int EmergencyPositionIterations = 1;

        public string SettingsKey = "F10";
    }
}
