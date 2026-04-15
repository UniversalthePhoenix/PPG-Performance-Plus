using PPGPerformancePlus.Config;
using PPGPerformancePlus.Core;
using PPGPerformancePlus.Integration;
using PPGPerformancePlus.Services;
using PPGPerformancePlus.Systems;

namespace PPGPerformancePlus;

public sealed class ModEntryPoint
{
    private readonly SystemCollection _systems = new();
    private ModContext? _context;

    public void Boot(string configPath, IGameBridge? gameBridge = null, ILogger? logger = null)
    {
        logger ??= new ConsoleLogger();
        gameBridge ??= new NullGameBridge();

        var configManager = new ConfigManager(configPath);
        var config = configManager.Load();

        _context = new ModContext(config, configManager, logger, gameBridge);

        _systems.Add(new ModProfiler());
        _systems.Add(new FrameMonitorSystem());
        _systems.Add(new SpawnManager());
        _systems.Add(new PhysicsController());
        _systems.Add(new LagDetector());
        _systems.Add(new RecoverySystem());
        _systems.Add(new UIManager());

        logger.Info("PPG Performance+ booting silently.");
        _systems.InitializeAll(_context);
    }

    public void Tick(TimeSpan deltaTime)
    {
        _systems.UpdateAll(deltaTime);
    }

    public void Shutdown()
    {
        _systems.ShutdownAll();
    }
}
