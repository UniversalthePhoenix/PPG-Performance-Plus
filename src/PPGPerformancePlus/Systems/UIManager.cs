using PPGPerformancePlus.Core;

namespace PPGPerformancePlus.Systems;

public sealed class UIManager : IModSystem
{
    private ModContext? _context;
    private bool _settingsShownForFirstRun;

    public string Name => nameof(UIManager);

    public void Initialize(ModContext context)
    {
        _context = context;

        if (context.Config.FirstRun)
        {
            context.GameBridge.OpenSettingsUi();
            _settingsShownForFirstRun = true;
            context.Config.FirstRun = false;
            context.ConfigManager.Save(context.Config);
        }
    }

    public void Update(TimeSpan deltaTime)
    {
        if (_context is null)
        {
            return;
        }

        if (_settingsShownForFirstRun)
        {
            _context.Logger.Info($"Settings UI opened on first run. Keybind: {_context.Config.SettingsKeybind}");
            _settingsShownForFirstRun = false;
        }
    }

    public void Shutdown()
    {
    }
}
