using System;
using Dalamud.Logging;
using Dalamud.Utility.Signatures;
using MemoryMarker.Utilities;

namespace MemoryMarker.System;

public class WaymarkManager : IDisposable
{
    public WaymarkManager()
    {
        SignatureHelper.Initialise(this);

        Service.ClientState.TerritoryChanged += OnZoneChange;
    }

    public void Dispose()
    {
        Service.ClientState.TerritoryChanged -= OnZoneChange;
    }
    
    private void OnZoneChange(object? sender, ushort territoryType)
    {
        if (Service.ClientState.IsPvP) return;
        
        MemoryHelper.Instance.SaveMarkerData();
        
        if (CompabilityHelper.IsWaymarkPresetInstalled())
        {
            PluginLog.Information(CompabilityHelper.WaymarkPresetWarning);
        }
        else
        {
            LoadMarkerData(territoryType);
        }
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
}