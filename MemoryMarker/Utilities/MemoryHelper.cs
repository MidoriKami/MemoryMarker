using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using MemoryMarker.DataModels;

namespace MemoryMarker.Utilities;

public unsafe class MemoryHelper
{
    private static MemoryHelper? _instance;
    public static MemoryHelper Instance => _instance ??= new MemoryHelper();

    private static FieldMarkerModule* FieldMarkers => FieldMarkerModule.Instance();
    
    private const int MarkerCount = 30;
    
    public void SaveMarkerData()
    {
        var territories = GetMarkerTerritories().ToList();

        if (territories.Count is 0)
        {
            SaveMarkerDataForTerritory(Service.ClientState.TerritoryType, false);
        }
        else
        {
            foreach (var territory in GetMarkerTerritories())
            {
                SaveMarkerDataForTerritory(territory);
            }
        }
    }
    
    private IEnumerable<uint> GetMarkerTerritories()
    {
        return Enumerable.Range(0, MarkerCount)
            .Select(index => FieldMarkers->PresetArraySpan[index].GetTerritoryId())
            .Where(territory => territory is not 0)
            .Distinct();
    }

    private void SaveMarkerDataForTerritory(uint targetArea, bool createIfNotFound = true)
    {
        // If we have saved markers
        if (Service.Configuration.FieldMarkerData.TryGetValue(targetArea, out var value))
        {
            // Overwrite Existing Markers
            var markers = GetZoneMarkerData(targetArea);
            value.UpdateMarkerData(markers.MarkerData);
            Service.Configuration.Save();
            PluginLog.Debug($"[Territory: {targetArea}] Saving Waymarks, Count: {Service.Configuration.FieldMarkerData[targetArea].GetMarkerCount()}");
        }
        else if (createIfNotFound)
        {
            // Create and Save Markers
            var markers = GetZoneMarkerData(targetArea);

            if (markers.GetMarkerCount() > 0)
            {
                Service.Configuration.FieldMarkerData.Add(targetArea, markers);
                Service.Configuration.Save();
                PluginLog.Debug($"[Territory: {targetArea}] Saving Waymarks, Count: {Service.Configuration.FieldMarkerData[targetArea].GetMarkerCount()}");
            }
        } 
    }
    
    private ZoneMarkerData GetZoneMarkerData(uint targetArea)
    {
        var markers = FieldMarkers->PresetArraySpan;
        var newZoneData = new ZoneMarkerData
        {
            MarkerData = new NamedMarker[MarkerCount],
        };

        // Check each of the current waymarks if they match the target TerritoryType
        foreach (var index in Enumerable.Range(0, MarkerCount))
        {
            if (markers[index].GetTerritoryId() != targetArea) continue;

            newZoneData.MarkerData[index] = new NamedMarker
            {
                Marker = markers[index],
                Name = string.Empty,
            };
        }

        return newZoneData;
    }

    public static void SetZoneMarkerData(ZoneMarkerData data)
    {
        foreach (var index in Enumerable.Range(0, data.MarkerData.Length))
        {
            var savedMarker = data.MarkerData[index]?.Marker;
            var targetAddress = (FieldMarkerPreset*)FieldMarkers->PresetArray + index;
            
            if (savedMarker is null)
            {
                Marshal.Copy(new byte[sizeof(FieldMarkerPreset)], 0, (nint)targetAddress, sizeof(FieldMarkerPreset));
            }
            else
            {
                Marshal.StructureToPtr(savedMarker, (nint)targetAddress, false);
            }
        }
    }

    public static void ClearZoneMarkerData()
    {
        foreach (var index in Enumerable.Range(0, MarkerCount))
        {
            var targetAddress = (FieldMarkerPreset*)FieldMarkers->PresetArray + index;
            
            Marshal.Copy(new byte[sizeof(FieldMarkerPreset)], 0, (nint)targetAddress, sizeof(FieldMarkerPreset));
        }
    }
}