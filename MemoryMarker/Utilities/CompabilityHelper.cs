using System.Linq;

namespace MemoryMarker.Utilities;

public static class CompabilityHelper
{
    public static bool IsWaymarkPresetInstalled() => Service.PluginInterface.PluginInternalNames.Any(internalName => internalName == "WaymarkPresetPlugin");
    public const string WaymarkPresetWarning = "WaymarkPreset plugin detected, skipping writing waymarks to memory.";
}