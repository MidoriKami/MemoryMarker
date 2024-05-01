using System;
using System.Linq;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.Interop;
using KamiLib.Classes;
using KamiLib.Window;
using Lumina.Excel.GeneratedSheets;

namespace MemoryMarker;

public class MemoryMarkerSystem : IDisposable {
    public static Configuration Configuration { get; private set; } = null!;
    public static AddonFieldMarkerContextMenu ContextMenu { get; private set; } = null!;
    public static AddonFieldMarkerController FieldMarkerController { get; private set; } = null!;
    public static WindowManager WindowManager { get; set; } = null!;
    
    public MemoryMarkerSystem() {
        Configuration = Service.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        ContextMenu = new AddonFieldMarkerContextMenu();
        FieldMarkerController = new AddonFieldMarkerController();
        WindowManager = new WindowManager(Service.PluginInterface);

        if (Service.Condition.IsBoundByDuty()) OnZoneChange(Service.ClientState.TerritoryType);

        Service.ClientState.TerritoryChanged += OnZoneChange;
    }

    public void Dispose() {
        Service.ClientState.TerritoryChanged -= OnZoneChange;

        ContextMenu.Dispose();
        FieldMarkerController.Dispose();
        WindowManager.Dispose();
    }

    private void OnZoneChange(ushort territoryType) {
        if (Service.ClientState.IsPvP) return;

        if (Service.PluginInterface.InstalledPlugins.Any(pluginInfo => pluginInfo is { InternalName: "WaymarkPresetPlugin", IsLoaded: true })) {
            Service.Log.Information("WaymarkPreset plugin detected, skipping writing waymarks to memory");
        }

        // If we are bound by duty after changing zones, we need to either generate new markers data, or load existing.
        else if (Service.Condition.IsBoundByDuty()) {
            TryImportMarkers();

            if (Configuration.FieldMarkerData.TryAdd(territoryType, new ZoneMarkerData())) {
                Service.Log.Debug($"No markers for {territoryType}, creating");
                Configuration.Save(false);
            }

            var markersForTerritory = Configuration.FieldMarkerData[territoryType];

            Service.Log.Info($"[Territory: {territoryType,4}] Loading Waymarks, Count: {markersForTerritory.Count}");
            SetZoneMarkerData(markersForTerritory);
        }
    }

    private unsafe void TryImportMarkers() {
        var markersChanged = false;

        foreach (var index in Enumerable.Range(0, FieldMarkerModule.Instance()->PresetArraySpan.Length)) {
            var marker = FieldMarkerModule.Instance()->PresetArraySpan.GetPointer(index);
            if (marker->ContentFinderConditionId is 0) continue;

            // Markers store ContentFinderConditions, we store TerritoryTypes, need to convert.
            if (Service.DataManager.GetExcelSheet<ContentFinderCondition>()?.GetRow(marker->ContentFinderConditionId) is not { TerritoryType.Row: var territoryType }) continue;
            if (territoryType is 0) continue;

            // Add a ZoneMarkerData entry for this territory if we don't have one already, do nothing if we do have one already.
            Configuration.FieldMarkerData.TryAdd(territoryType, new ZoneMarkerData());

            // If our copy of this slot is null, we need to read the marker data and save it.
            if (Configuration.FieldMarkerData[territoryType].MarkerData[index] is null) {
                Service.Log.Debug($"[Territory: {territoryType,4}] New Waymark Found, Index {index}");
                Configuration.FieldMarkerData[territoryType].MarkerData[index] = new NamedMarker {
                    Marker = *marker,
                    Name = string.Empty
                };

                markersChanged = true;
            }
        }

        if (markersChanged) Configuration.Save();
    }

    private static unsafe void SetZoneMarkerData(ZoneMarkerData data) {
        foreach (var index in Enumerable.Range(0, data.MarkerData.Length)) {
            var namedMarker = data.MarkerData[index];
            var targetAddress = FieldMarkerModule.Instance()->PresetArraySpan.GetPointer(index);

            if (namedMarker is not null) {
                Service.Log.Debug($"[Territory: {Service.ClientState.TerritoryType,4}] [{index,2}] Loaded '{(namedMarker.Name.IsNullOrEmpty() ? "Unnamed" : namedMarker.Name)}'");
                *targetAddress = namedMarker.Marker;
            }
            else {
                *targetAddress = default;
            }
        }
    }
}