using Dalamud.ContextMenu;
using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using MemoryMarker.Utilities;
using MemoryMarker.Windows;

namespace MemoryMarker.Controllers;

public unsafe class FieldMarkerContextMenu
{

    private static readonly SeString ContextMenuLabel = new SeStringBuilder().AddText("Rename").Build();
    private readonly DalamudContextMenu contextMenu;
    private readonly GameObjectContextMenuItem contextMenuItem;

    public FieldMarkerContextMenu()
    {
        contextMenu = new DalamudContextMenu();
        contextMenuItem = new GameObjectContextMenuItem(ContextMenuLabel, RenameContextMenuAction, true);
        contextMenu.OnOpenGameObjectContextMenu += OpenGameObjectContextMenu;
    }

    public void Dispose()
    {
        contextMenu.OnOpenGameObjectContextMenu -= OpenGameObjectContextMenu;
        contextMenu.Dispose();
    }

    private void OpenGameObjectContextMenu(GameObjectContextMenuOpenArgs args)
    {
        if (args.ParentAddonName != "FieldMarker") return;

        args.AddCustomItem(contextMenuItem);
    }

    private void RenameContextMenuAction(GameObjectContextMenuItemSelectedArgs args)
    {
        MemoryHelper.Instance.SaveMarkerData();

        // Check that we have saved config for this territory
        if (MemoryMarkerSystem.Configuration.FieldMarkerData.TryGetValue(Service.ClientState.TerritoryType, out var value))
        {
            var agentFieldMarker = (AgentFieldMarker*) AgentModule.Instance()->GetAgentByInternalId(AgentId.FieldMarker);
            var slotClicked = agentFieldMarker->PageIndexOffset;

            // Check that the preset we are modifying exists
            if (value.MarkerData[slotClicked] is not null)
            {
                RenameWindow.ShowWindow();
            }
        }
    }
}