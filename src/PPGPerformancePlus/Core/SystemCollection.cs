namespace PPGPerformancePlus.Core;

public sealed class SystemCollection
{
    private readonly List<IModSystem> _systems = new();

    public IReadOnlyList<IModSystem> Systems => _systems;

    public void Add(IModSystem system) => _systems.Add(system);

    public void InitializeAll(ModContext context)
    {
        foreach (var system in _systems)
        {
            context.Logger.Info($"Initializing {system.Name}");
            system.Initialize(context);
        }
    }

    public void UpdateAll(TimeSpan deltaTime)
    {
        foreach (var system in _systems)
        {
            system.Update(deltaTime);
        }
    }

    public void ShutdownAll()
    {
        for (var index = _systems.Count - 1; index >= 0; index--)
        {
            _systems[index].Shutdown();
        }
    }
}
