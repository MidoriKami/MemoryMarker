using System;
using Dalamud.Logging;
using Dalamud.Utility.Signatures;
using KamiLib.ChatCommands;
using MemoryMarker.Utilities;

namespace MemoryMarker.System;

public class WaymarkManager : IDisposable
{
    private uint previousTerritoryType;
    
    public WaymarkManager()
    {
        SignatureHelper.Initialise(this);

        Service.ClientState.TerritoryChanged += OnZoneChange;
    }

    public void Dispose()
    {
        Service.ClientState.TerritoryChanged -= OnZoneChange;
    }
    
    private void OnZoneChange(object? sender, ushort e)
    {
        if (Service.ClientState.IsPvP) return;
        
        if (CompabilityHelper.IsWaymarkPresetInstalled())
        {
            Chat.PrintError(CompabilityHelper.WaymarkPresetWarning);
            return;
        }

        SaveMarkerData(e);
        LoadMarkerData(e);
    }
    
    private void LoadMarkerData(ushort territoryType)
    {
        // If we have saved markers
        if (Service.Configuration.FieldMarkerData.TryGetValue(territoryType, out var markers))
        {
            MemoryHelper.Instance.SetZoneMarkerData(markers);
            PluginLog.Debug($"Marker data found for zone {territoryType}, loading");
        }
        
        // If not lets clear invalid marker data
        else
        {
            MemoryHelper.Instance.ClearZoneMarkerData();
            PluginLog.Debug($"Marker data not found, clearing waymarks");
        }
    }

    private void SaveMarkerData(ushort territoryType)
    {
        if (previousTerritoryType is not 0)
        {
            MemoryHelper.Instance.SaveZoneMarkerData(previousTerritoryType);
        }

        previousTerritoryType = territoryType;
    }
}