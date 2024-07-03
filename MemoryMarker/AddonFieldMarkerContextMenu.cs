using Dalamud.Game.Gui.ContextMenu;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using KamiLib.Window;

namespace MemoryMarker;

public unsafe class AddonFieldMarkerContextMenu {
    public AddonFieldMarkerContextMenu() {
        Service.ContextMenu.OnMenuOpened += OnContextMenuOpened;
    }

    public void Dispose() {
        Service.ContextMenu.OnMenuOpened -= OnContextMenuOpened;
    }

    private void OnContextMenuOpened(IMenuOpenedArgs args) {
        if (args is { AddonName: "FieldMarker" }) {

            // Just don't even add the Rename button if the player is in PvP
            if (Service.ClientState.IsPvP) return;

            var slotClicked = AgentFieldMarker.Instance()->PageIndexOffset;
            ref var slotMarkerData = ref FieldMarkerModule.Instance()->Presets[slotClicked];

            args.AddMenuItem(new MenuItem {
                Name = "Rename",
                OnClicked = RenameContextMenuAction,
                UseDefaultPrefix = true,
                IsEnabled =
                    GameMain.Instance()->CurrentContentFinderConditionId == slotMarkerData.ContentFinderConditionId &&
                    GameMain.Instance()->CurrentContentFinderConditionId is not 0 &&
                    slotMarkerData.ContentFinderConditionId is not 0,
            });
        }
    }

    private void RenameContextMenuAction(IMenuItemClickedArgs menuItemClickedArgs) {
        var agentFieldMarker = (AgentFieldMarker*) menuItemClickedArgs.AgentPtr;
        var slotClicked = agentFieldMarker->PageIndexOffset;

        // Check that we have saved config for this territory
        if (System.Configuration.FieldMarkerData.TryGetValue(Service.ClientState.TerritoryType, out var value)) {
            // Check that the preset we are modifying exists
            if (value.MarkerData[slotClicked] is not null) {
                System.WindowManager.AddWindow(new RenameWindow(), WindowFlags.OpenImmediately);
            }
        }
    }
}