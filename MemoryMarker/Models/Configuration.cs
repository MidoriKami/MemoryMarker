﻿using System.Collections.Generic;
using Dalamud.Configuration;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace MemoryMarker;

public class NamedMarker
{
    public string Name = string.Empty;
    public FieldMarkerPreset Marker { get; init; }
}

public class ZoneMarkerData
{
    public NamedMarker?[] MarkerData { get; init; } = null!;
}

public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public Dictionary<uint, ZoneMarkerData> FieldMarkerData = new();

    public void Save() => Service.PluginInterface.SavePluginConfig(this);
}