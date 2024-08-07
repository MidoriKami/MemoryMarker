﻿using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Dalamud.Configuration;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

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

public class Configuration : IPluginConfiguration {
    public int Version { get; set; } = 3;
    
    public Dictionary<uint, ZoneMarkerData> FieldMarkerData = [];

    public void Save(bool prune = true) {
        if (prune) FieldMarkerData = FieldMarkerData.Where(dataPair => dataPair.Value.Count is not 0).ToDictionary();

        Service.PluginInterface.SavePluginConfig(this);

        Service.Log.Verbose($"Saving {FieldMarkerData.Count} Territories");
        foreach (var (territory, zoneMarkerData) in FieldMarkerData) {
            Service.Log.Verbose($"[{territory,4}] Saving {zoneMarkerData.Count} Markers");
        }
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 0, Size = 0x68)]
public struct FieldMarkerPreset {
    public GamePresetPoint A;
    public GamePresetPoint B;
    public GamePresetPoint C;
    public GamePresetPoint D;
    public GamePresetPoint One;
    public GamePresetPoint Two;
    public GamePresetPoint Three;
    public GamePresetPoint Four;
    public byte ActiveMarkers;
    public byte Reserved;
    public ushort ContentFinderConditionId;
    public int Timestamp;
}
