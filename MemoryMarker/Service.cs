using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Plugin;

namespace MemoryMarker;

internal class Service
{
    [PluginService] public static DalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] public static ChatGui Chat { get; private set; } = null!;

    public static Configuration Configuration { get; set; } = null!;
}