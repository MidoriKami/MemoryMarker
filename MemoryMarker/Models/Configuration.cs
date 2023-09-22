using System;
using System.Collections.Generic;
using Dalamud.Configuration;
using MemoryMarker.DataModels;

namespace MemoryMarker;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public Dictionary<uint, ZoneMarkerData> FieldMarkerData = new();

    public void Save() => Service.PluginInterface.SavePluginConfig(this);
}