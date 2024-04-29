using Dalamud.Game.Gui.ContextMenu;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace MemoryMarker;

public unsafe class AddonFieldMarkerContextMenu {
    public AddonFieldMarkerContextMenu() {
        Service.ContextMenu.OnMenuOpened += OnContextMenuOpened;
    }

    public void Dispose() {
        Service.ContextMenu.OnMenuOpened -= OnContextMenuOpened;
    }

    private void OnContextMenuOpened(MenuOpenedArgs args) {
        if (args is { AddonName: "FieldMarker" }) {

            // Just don't even add the Rename button if the player is in PvP
            if (Service.ClientState.IsPvP) return;

            var slotClicked = AgentFieldMarker.Instance()->PageIndexOffset;
            ref var slotMarkerData = ref FieldMarkerModule.Instance()->PresetArraySpan[slotClicked];

            args.AddMenuItem(new MenuItem {
                Name = "Rename",
                OnClicked = RenameContextMenuAction,
                Prefix = MenuItem.DalamudDefaultPrefix,
                PrefixColor = MenuItem.DalamudDefaultPrefixColor,
                IsEnabled =
                    GameMain.Instance()->CurrentContentFinderConditionId == slotMarkerData.ContentFinderConditionId &&
                    GameMain.Instance()->CurrentContentFinderConditionId is not 0 &&
                    slotMarkerData.ContentFinderConditionId is not 0
            });
        }
    }

    private void RenameContextMenuAction(MenuItemClickedArgs menuItemClickedArgs) {
        var agentFieldMarker = (AgentFieldMarker*) menuItemClickedArgs.AgentPtr;
        var slotClicked = agentFieldMarker->PageIndexOffset;

        // Check that we have saved config for this territory
        if (MemoryMarkerSystem.Configuration.FieldMarkerData.TryGetValue(Service.ClientState.TerritoryType, out var value)) {
            // Check that the preset we are modifying exists
            if (value.MarkerData[slotClicked] is not null) {
                RenameWindow.ShowWindow();
            }
        }
    }
}