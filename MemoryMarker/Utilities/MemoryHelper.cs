using System.Linq;
using System.Runtime.InteropServices;
using Dalamud.Logging;
using Dalamud.Utility.Signatures;
using MemoryMarker.DataModels;

namespace MemoryMarker.Utilities;

public unsafe class MemoryHelper
{
    private static MemoryHelper? _instance;
    public static MemoryHelper Instance => _instance ??= new MemoryHelper();

    private delegate nint GetFieldMarkerDataSection(nint pConfigFile = 0, byte sectionIndex = 0x11);
    private delegate FieldMarkerStruct* GetFieldMarkerData(nint uiSaveSegmentAddress, int slot);

    [Signature("40 53 48 83 EC 20 48 8B 0D ?? ?? ?? ?? 0F B7 DA E8 ?? ?? ?? ?? 4C 8B C0")]
    private readonly GetFieldMarkerDataSection? getUiSaveSegmentAddress = null;

    [Signature("4C 8B C9 85 D2 78 0A")] 
    private readonly GetFieldMarkerData? getFieldMarkerPresetForSlot = null;

    private const int MarkerCount = 30;

    private MemoryHelper()
    {
        SignatureHelper.Initialise(this);
    }

    private FieldMarkerStruct* GetFieldMarker(int slot)
    {
        var segmentAddress = getUiSaveSegmentAddress?.Invoke();
        if (segmentAddress is null) return null;
        
        if (getFieldMarkerPresetForSlot is null) return null;
        
        return getFieldMarkerPresetForSlot.Invoke(segmentAddress.Value, slot);
    }

    private FieldMarkerStruct[] GetFieldMarkers()
    {
        return Enumerable.Range(0, MarkerCount).Select(i => *GetFieldMarker(i)).ToArray();
    }

    public void SaveZoneMarkerData(uint targetArea)
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
        else
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
        var markers = GetFieldMarkers();
        var newZoneData = new ZoneMarkerData
        {
            MarkerData =  new NamedMarker[MarkerCount],
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

    public void SetZoneMarkerData(ZoneMarkerData data)
    {
        foreach (var index in Enumerable.Range(0, data.MarkerData.Length))
        {
            var savedMarker = data.MarkerData[index]?.Marker;
            var targetAddress = GetFieldMarker(index);
            
            if (savedMarker is null)
            {
                Marshal.Copy(new byte[sizeof(FieldMarkerStruct)], 0, (nint)targetAddress, sizeof(FieldMarkerStruct));
            }
            else
            {
                Marshal.StructureToPtr(savedMarker, (nint) targetAddress, false);
            }
        }
    }

    public void ClearZoneMarkerData()
    {
        foreach (var index in Enumerable.Range(0, MarkerCount))
        {
            var targetAddress = GetFieldMarker(index);
            
            Marshal.Copy(new byte[sizeof(FieldMarkerStruct)], 0, (nint)targetAddress, sizeof(FieldMarkerStruct));
        }
    }
}