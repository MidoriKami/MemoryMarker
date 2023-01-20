using Dalamud.Plugin;
using KamiLib;
using MemoryMarker.System;
using MemoryMarker.Windows;

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
        
        KamiCommon.WindowManager.AddWindow(new DebugWindow());
    }

    public void Dispose()
    {
        KamiCommon.Dispose();
        
        Service.WaymarkManager.Dispose();
    }
}