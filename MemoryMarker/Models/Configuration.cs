using System.Collections.Generic;
using System.Linq;
using Dalamud.Configuration;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace MemoryMarker;

public class NamedMarker
{
    public string Name = string.Empty;
    public FieldMarkerPreset Marker { get; set; }
}

public class ZoneMarkerData
{
    private const int WaymarkCount = 30;
    
    public NamedMarker?[] MarkerData { get; init; } = new NamedMarker?[WaymarkCount];

    public int Count => MarkerData.OfType<NamedMarker>().Count();
}

public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public Dictionary<uint, ZoneMarkerData> FieldMarkerData = new();

    public void Save()
    {
        Prune();
        
        Service.PluginInterface.SavePluginConfig(this);
        
        Service.Log.Verbose($"Saving {FieldMarkerData.Count} Territories");
        foreach (var (territory, zoneMarkerData) in FieldMarkerData)
        {
            Service.Log.Verbose($"[{territory,4}] Saving {zoneMarkerData.Count} Markers");
        }
    }

    private void Prune()
    {
        var emptyTerritories = new List<uint>();

        foreach (var (territory, zoneData) in FieldMarkerData)
        {
            if (zoneData.Count is 0) emptyTerritories.Add(territory);
        }

        foreach (var territory in emptyTerritories)
        {
            FieldMarkerData.Remove(territory);
        }
    }
}