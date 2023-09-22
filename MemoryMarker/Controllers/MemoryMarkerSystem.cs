using System;
using MemoryMarker.System;

namespace MemoryMarker.Controllers;

public class MemoryMarkerSystem : IDisposable
{
    public static Configuration Configuration { get; private set; } = null!;
    public static WaymarkController WaymarkController { get; private set; } = null!;
    public static FieldMarkerContextMenu ContextMenu { get; private set; } = null!;
    public static AddonFieldMarkerController FieldMarkerController { get; private set; } = null!;

    public MemoryMarkerSystem()
    {
        Configuration = Service.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        WaymarkController = new WaymarkController();
        ContextMenu = new FieldMarkerContextMenu();
        FieldMarkerController = new AddonFieldMarkerController();
    }

    public void Dispose()
    {
        WaymarkController.Dispose();
        ContextMenu.Dispose();
        FieldMarkerController.Dispose();
    }
}