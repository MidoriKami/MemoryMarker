using System.Linq;

namespace MemoryMarker.Utilities;

public static class CompabilityHelper
{
    public static bool IsWaymarkPresetInstalled() => Service.PluginInterface.InstalledPlugins.Any(pluginInfo => pluginInfo is { InternalName: "WaymarkPresetPlugin", IsLoaded: true });
    public const string WaymarkPresetWarning = "WaymarkPreset plugin detected, skipping writing waymarks to memory.";
}