using System;
using System.Linq;
using System.Runtime.InteropServices;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.Interop;
using KamiLib.Game;

namespace MemoryMarker.Controllers;

public class MemoryMarkerSystem : IDisposable
{
    public static Configuration Configuration { get; private set; } = null!;
    public static AddonFieldMarkerContextMenu ContextMenu { get; private set; } = null!;
    public static AddonFieldMarkerController FieldMarkerController { get; private set; } = null!;

    public MemoryMarkerSystem()
    {
        Configuration = Service.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        ContextMenu = new AddonFieldMarkerContextMenu();
        FieldMarkerController = new AddonFieldMarkerController();
        
        if (Condition.IsBoundByDuty()) OnZoneChange(Service.ClientState.TerritoryType);
        
        Service.ClientState.TerritoryChanged += OnZoneChange;
    }

    private void OnZoneChange(ushort territoryType)
    {
        if (Service.ClientState.IsPvP) return;

        if (Service.PluginInterface.InstalledPlugins.Any(pluginInfo => pluginInfo is { InternalName: "WaymarkPresetPlugin", IsLoaded: true }))
        {
            Service.Log.Information("WaymarkPreset plugin detected, skipping writing waymarks to memory");
        }
        
        // If we are bound by duty after changing zones, we need to either generate new markers data, or load existing.
        else if (Condition.IsBoundByDuty())
        {
            if (Configuration.FieldMarkerData.TryAdd(territoryType, new ZoneMarkerData()))
            {
                Service.Log.Debug($"No markers for {territoryType}, creating");
                Configuration.Save(false);
            }

            var markersForTerritory = Configuration.FieldMarkerData[territoryType];

            Service.Log.Info($"[Territory: {territoryType, 4}] Loading Waymarks, Count: {markersForTerritory.Count}");
            SetZoneMarkerData(markersForTerritory);
        }
    }

    private static unsafe void SetZoneMarkerData(ZoneMarkerData data)
    {
        foreach (var index in Enumerable.Range(0, data.MarkerData.Length))
        {
            var namedMarker = data.MarkerData[index];
            var targetAddress = FieldMarkerModule.Instance()->PresetArraySpan.GetPointer(index);

            if (namedMarker is not null)
            {
                Service.Log.Debug($"[Territory: {Service.ClientState.TerritoryType, 4}] [{index,2}] Loaded '{(namedMarker.Name.IsNullOrEmpty() ? "Unnamed" : namedMarker.Name)}'");
                Marshal.StructureToPtr(namedMarker.Marker, (nint) targetAddress, false);
            }
            else
            {
                Marshal.Copy(new byte[sizeof(FieldMarkerPreset)], 0, (nint) targetAddress, sizeof(FieldMarkerPreset));
            }
        }
    }
    
    public void Dispose()
    {
        Service.ClientState.TerritoryChanged -= OnZoneChange;
        
        ContextMenu.Dispose();
        FieldMarkerController.Dispose();
    }
}