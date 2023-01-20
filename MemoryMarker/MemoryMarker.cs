using Dalamud.Plugin;
using KamiLib;
using MemoryMarker.Addons;
using MemoryMarker.System;

namespace MemoryMarker;

public sealed class Plugin : IDalamudPlugin
{
    public string Name => "MemoryMarker";
    
    public Plugin(DalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Service>();
        
        KamiCommon.Initialize(pluginInterface, Name, () => Service.Configuration.Save());
        
        Service.Configuration = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Service.Configuration.Initialize(pluginInterface);

        Service.WaymarkManager = new WaymarkManager();
        Service.AddonFieldMarker = new AddonFieldMarker();
    }

    public void Dispose()
    {
        KamiCommon.Dispose();
        
        Service.WaymarkManager.Dispose();
        Service.AddonFieldMarker.Dispose();
    }
}