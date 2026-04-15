using PPGPerformancePlus.Config;
using PPGPerformancePlus.Integration;
using PPGPerformancePlus.Services;

namespace PPGPerformancePlus.Core;

public sealed class ModContext
{
    private readonly Dictionary<Type, object> _services = new();

    public ModContext(
        ModConfig config,
        ConfigManager configManager,
        ILogger logger,
        IGameBridge gameBridge)
    {
        Config = config;
        ConfigManager = configManager;
        Logger = logger;
        GameBridge = gameBridge;
    }

    public ModConfig Config { get; }
    public ConfigManager ConfigManager { get; }
    public ILogger Logger { get; }
    public IGameBridge GameBridge { get; }

    public void RegisterService<TService>(TService service) where TService : class
    {
        _services[typeof(TService)] = service;
    }

    public TService GetRequiredService<TService>() where TService : class
    {
        if (_services.TryGetValue(typeof(TService), out var service) && service is TService typedService)
        {
            return typedService;
        }

        throw new InvalidOperationException($"Service not registered: {typeof(TService).Name}");
    }
}
