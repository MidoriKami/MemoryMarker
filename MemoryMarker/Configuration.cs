using System;
using System.Collections.Generic;
using Dalamud.Configuration;
using Dalamud.Plugin;
using MemoryMarker.DataModels;

namespace MemoryMarker;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public Dictionary<uint, ZoneMarkerData> FieldMarkerData = new();

    [NonSerialized]
    private DalamudPluginInterface? pluginInterface;
    public void Initialize(DalamudPluginInterface dalamudInterface) => pluginInterface = dalamudInterface;
    public void Save() => pluginInterface!.SavePluginConfig(this);
}