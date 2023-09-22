using Dalamud.Plugin;
using KamiLib;
using MemoryMarker.Controllers;

namespace MemoryMarker;

public sealed class MemoryMarkerPlugin : IDalamudPlugin
{
    public string Name => "MemoryMarker";
    
    public static MemoryMarkerSystem System = null!;

    public MemoryMarkerPlugin(DalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Service>();

        KamiCommon.Initialize(pluginInterface, Name);

        System = new MemoryMarkerSystem();
    }

    public void Dispose()
    {
        KamiCommon.Dispose();

        System.Dispose();
    }
}