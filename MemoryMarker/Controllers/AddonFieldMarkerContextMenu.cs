using Dalamud.Game.Gui.ContextMenu;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using MemoryMarker.Windows;

namespace MemoryMarker.Controllers;

public unsafe class AddonFieldMarkerContextMenu {
    public AddonFieldMarkerContextMenu() {
        Service.ContextMenu.OnMenuOpened += OnContextMenuOpened;
    }

    public void Dispose() {
        Service.ContextMenu.OnMenuOpened -= OnContextMenuOpened;
    }

    private void OnContextMenuOpened(MenuOpenedArgs args) {
        if (args is { AddonName: "FieldMarker" }) {
            args.AddMenuItem(new MenuItem{
                Name = "Rename",
                OnClicked = RenameContextMenuAction,
            });
        }
    }

    private void RenameContextMenuAction(MenuItemClickedArgs menuItemClickedArgs) {
        // Check that we have saved config for this territory
        if (MemoryMarkerSystem.Configuration.FieldMarkerData.TryGetValue(Service.ClientState.TerritoryType, out var value)) {
            var agentFieldMarker = (AgentFieldMarker*) menuItemClickedArgs.AgentPtr;
            var slotClicked = agentFieldMarker->PageIndexOffset;

            // Check that the preset we are modifying exists
            if (value.MarkerData[slotClicked] is not null) {
                RenameWindow.ShowWindow();
            }
        }
    }
}