namespace PPGPerformancePlus.Services;

public sealed class LagHistory
{
    private readonly List<LagIncident> _incidents = new();

    public IReadOnlyList<LagIncident> Incidents => _incidents;

    public void Add(LagIncident incident) => _incidents.Add(incident);
}
