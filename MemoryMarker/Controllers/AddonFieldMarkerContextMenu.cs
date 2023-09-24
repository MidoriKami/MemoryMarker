using Dalamud.ContextMenu;
using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using MemoryMarker.Windows;

namespace MemoryMarker.Controllers;

public unsafe class AddonFieldMarkerContextMenu
{
    private static readonly SeString ContextMenuLabel = new SeStringBuilder().AddText("Rename").Build();
    private readonly DalamudContextMenu contextMenu;
    private readonly GameObjectContextMenuItem contextMenuItem;

    public AddonFieldMarkerContextMenu()
    {
        contextMenu = new DalamudContextMenu(Service.PluginInterface);
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
        if (args is { ParentAddonName: "FieldMarker" })
        {
            args.AddCustomItem(contextMenuItem);
        }
    }

    private void RenameContextMenuAction(GameObjectContextMenuItemSelectedArgs args)
    {
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