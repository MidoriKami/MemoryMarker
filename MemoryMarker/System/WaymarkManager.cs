using System;
using KamiLib.ChatCommands;
using KamiLib.GameState;
using MemoryMarker.Utilities;

namespace MemoryMarker.System;

public class WaymarkManager : IDisposable
{
    public WaymarkManager()
    {
        Service.ClientState.TerritoryChanged += OnZoneChange;
    }

    public void Dispose()
    {
        Service.ClientState.TerritoryChanged -= OnZoneChange;
    }
    
    private void OnZoneChange(object? sender, ushort e)
    {
        if (Service.ClientState.IsPvP) return;
        if (!Condition.IsBoundByDuty()) return;
        
        if (CompabilityHelper.IsWaymarkPresetInstalled())
        {
            Chat.PrintError("WaymarkPresetPlugin is not compatible with MemoryMarker. Please uninstall one of these plugins.");
            return;
        }
    }
}