using System.Linq;

namespace MemoryMarker.DataModels;

public class ZoneMarkerData
{
    public NamedMarker?[] MarkerData { get; init; } = null!;
    public uint TerritoryType { get; init; }
    public int GetMarkerCount() => MarkerData.Where(marker => marker is not null).Count();
}