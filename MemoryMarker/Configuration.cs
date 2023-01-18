using System;
using Dalamud.Configuration;
using Dalamud.Plugin;

namespace MemoryMarker;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;


    [NonSerialized]
    private DalamudPluginInterface? pluginInterface;
    public void Initialize(DalamudPluginInterface dalamudInterface) => pluginInterface = dalamudInterface;
    public void Save() => pluginInterface!.SavePluginConfig(this);
}