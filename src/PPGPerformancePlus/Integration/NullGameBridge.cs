namespace PPGPerformancePlus.Integration;

public sealed class NullGameBridge : IGameBridge
{
    public double CurrentFrameTimeMs => 0;
    public bool IsSimulationPaused => false;

    public void PauseSimulation()
    {
    }

    public void ResumeSimulation()
    {
    }

    public void ShowNotification(string title, string message, NotificationSeverity severity)
    {
    }

    public void OpenSettingsUi()
    {
    }
}