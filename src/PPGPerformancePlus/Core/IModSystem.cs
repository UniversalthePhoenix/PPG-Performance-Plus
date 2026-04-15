namespace PPGPerformancePlus.Core;

public interface IModSystem
{
    string Name { get; }
    void Initialize(ModContext context);
    void Update(TimeSpan deltaTime);
    void Shutdown();
}
