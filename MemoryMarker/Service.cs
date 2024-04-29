using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace MemoryMarker;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
internal class Service {
    [PluginService] public static DalamudPluginInterface PluginInterface { get; set; }
    [PluginService] public static IClientState ClientState { get; set; }
    [PluginService] public static IAddonLifecycle AddonLifecycle { get; set; }
    [PluginService] public static IPluginLog Log { get; set; }
    [PluginService] public static IContextMenu ContextMenu { get; set; }
    [PluginService] public static INotificationManager NotificationManager { get; set; }
    [PluginService] public static ICondition Condition { get; set; }
    [PluginService] public static IDataManager DataManager { get; set; }
    [PluginService] public static IChatGui ChatGui { get; set; }
}
