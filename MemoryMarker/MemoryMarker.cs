using Dalamud.Plugin;
using KamiLib;

namespace MemoryMarker;

public sealed class Plugin : IDalamudPlugin
{
    public string Name => "Memory Marker";
    
    public Plugin(DalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Service>();
        
        KamiCommon.Initialize(pluginInterface, Name, () => Service.Configuration.Save());
        
        Service.Configuration = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Service.Configuration.Initialize(pluginInterface);
    }

    public void Dispose()
    {
        KamiCommon.Dispose();
    }
}