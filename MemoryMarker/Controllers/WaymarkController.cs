using System;
using MemoryMarker.Controllers;
using MemoryMarker.Utilities;

namespace MemoryMarker.System;

public class WaymarkController : IDisposable
{
    public WaymarkController()
    {
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
            Service.Log.Information(CompabilityHelper.WaymarkPresetWarning);
        }
        else
        {
            LoadMarkerData(territoryType);
        }
    }

    private void LoadMarkerData(ushort territoryType)
    {
        // If we have saved markers
        if (MemoryMarkerSystem.Configuration.FieldMarkerData.TryGetValue(territoryType, out var markers))
        {
            MemoryHelper.SetZoneMarkerData(markers);
            Service.Log.Debug($"[Territory: {territoryType}] Loading Waymarks, Count: {markers.GetMarkerCount()}");
        }

        // If not lets clear invalid marker data
        else
        {
            MemoryHelper.ClearZoneMarkerData();
            Service.Log.Debug($"[Territory: {territoryType}] No Markers for Zone, clearing waymarks");
        }
    }
}