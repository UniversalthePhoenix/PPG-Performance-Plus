namespace PPGPerformancePlus.Integration;

public interface IGameBridge
{
    double CurrentFrameTimeMs { get; }
    bool IsSimulationPaused { get; }

    void PauseSimulation();
    void ResumeSimulation();
    void ShowNotification(string title, string message, NotificationSeverity severity);
    void OpenSettingsUi();
}
