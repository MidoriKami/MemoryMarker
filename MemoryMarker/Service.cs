using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Plugin;
using MemoryMarker.Addons;
using MemoryMarker.System;

namespace MemoryMarker;

internal class Service
{
    [PluginService] public static DalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] public static ClientState ClientState { get; private set; } = null!;
    [PluginService] public static GameGui GameGui { get; private set; } = null!;
    [PluginService] public static Framework Framework { get; private set; } = null!;

    public static Configuration Configuration { get; set; } = null!;
    public static WaymarkManager WaymarkManager { get; set; } = null!;
    public static FieldMarkerWindow FieldMarkerWindow { get; set; } = null!;
}