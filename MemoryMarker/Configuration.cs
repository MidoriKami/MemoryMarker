using System.Collections.Generic;
using System.Linq;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using KamiLib.Configuration;

namespace MemoryMarker;

public class NamedMarker {
    public string Name = string.Empty;
    public FieldMarkerPreset Marker { get; set; }
}

public class ZoneMarkerData {
    private const int WaymarkCount = 30;
    public NamedMarker?[] MarkerData { get; init; } = new NamedMarker?[WaymarkCount];
    public int Count => MarkerData.OfType<NamedMarker>().Count();
}

public class Configuration : CharacterConfiguration {
    public Dictionary<uint, ZoneMarkerData> FieldMarkerData = new();

    public void Save(bool prune = true) {
        if (prune) FieldMarkerData = FieldMarkerData.Where(dataPair => dataPair.Value.Count is not 0).ToDictionary();

        Service.PluginInterface.SavePluginConfig(this);

        Service.Log.Verbose($"Saving {FieldMarkerData.Count} Territories");
        foreach (var (territory, zoneMarkerData) in FieldMarkerData) {
            Service.Log.Verbose($"[{territory,4}] Saving {zoneMarkerData.Count} Markers");
        }
    }
}