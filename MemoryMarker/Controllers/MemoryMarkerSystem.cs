using System;
using System.Linq;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using KamiLib.Utilities;

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
        
        Service.ClientState.TerritoryChanged += OnZoneChange;
    }

    private void OnZoneChange(ushort territoryType)
    {
        if (Service.ClientState.IsPvP) return;

        if (Service.PluginInterface.InstalledPlugins.Any(pluginInfo => pluginInfo is { InternalName: "WaymarkPresetPlugin", IsLoaded: true }))
        {
            Service.Log.Information("WaymarkPreset plugin detected, skipping writing waymarks to memory.");
        }
        else
        {
            // If we have saved markers for this territory
            if (Configuration.FieldMarkerData.TryGetValue(territoryType, out var markers))
            {
                SetZoneMarkerData(markers);
                Service.Log.Debug($"[Territory: {territoryType}] Loading Waymarks, Count: {markers.MarkerData.OfType<NamedMarker>().Count()}");
            }
        }
    }

    private static unsafe void SetZoneMarkerData(ZoneMarkerData data)
    {
        foreach (var index in Enumerable.Range(0, data.MarkerData.Length))
        {
            var savedMarker = data.MarkerData[index]?.Marker;
            var targetAddress = FieldMarkerModule.Instance()->PresetArraySpan.Get(index);

            if (savedMarker is null)
            {
                Marshal.Copy(new byte[sizeof(FieldMarkerPreset)], 0, (nint) targetAddress, sizeof(FieldMarkerPreset));
            }
            else
            {
                Marshal.StructureToPtr(savedMarker, (nint) targetAddress, false);
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