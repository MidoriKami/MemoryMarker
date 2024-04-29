using Dalamud.Plugin;
using MemoryMarker.Controllers;

namespace MemoryMarker;

public sealed class MemoryMarkerPlugin : IDalamudPlugin {
    public static MemoryMarkerSystem System = null!;

    public MemoryMarkerPlugin(DalamudPluginInterface pluginInterface) {
        pluginInterface.Create<Service>();

        System = new MemoryMarkerSystem();
    }

    public void Dispose() {
        System.Dispose();
    }
}