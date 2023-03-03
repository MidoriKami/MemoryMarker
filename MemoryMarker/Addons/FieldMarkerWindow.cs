using System;
using System.Linq;
using Dalamud.ContextMenu;
using Dalamud.Game;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiLib.Atk;
using KamiLib.Hooking;
using MemoryMarker.Utilities;
using MemoryMarker.Windows;

namespace MemoryMarker.Addons;

public unsafe class FieldMarkerWindow : IDisposable
{
    private readonly DalamudContextMenu contextMenu;
    private readonly GameObjectContextMenuItem contextMenuItem;
    private static SeString ContextMenuLabel => new(new TextPayload("Rename"));

    private Hook<Delegates.Addon.Update>? onUpdateHook;
    private readonly Hook<Delegates.Agent.ReceiveEvent>? onReceiveEventHook;

    private static AddonFieldMarker* Addon => (AddonFieldMarker*) Service.GameGui.GetAddonByName("FieldMarker");
    private byte SelectedPage => Addon->SelectedPage;
    private int HoveredIndex => Addon->HoveredPresetIndex;
    private int lastHoveredIndex;
    
    public FieldMarkerWindow()
    {
        SignatureHelper.Initialise(this);
        
        contextMenu = new DalamudContextMenu();
        contextMenuItem = new GameObjectContextMenuItem(ContextMenuLabel, RenameContextMenuAction, true);
        contextMenu.OnOpenGameObjectContextMenu += OpenGameObjectContextMenu;

        var agent = AgentModule.Instance()->GetAgentByInternalId(AgentId.FieldMarker);

        onReceiveEventHook ??= Hook<Delegates.Agent.ReceiveEvent>.FromAddress(new nint(agent->VTable->ReceiveEvent), ReceiveEvent);
        onReceiveEventHook.Enable();
        
        Service.Framework.Update += OnFrameworkUpdate;
    }
    
    private void OnFrameworkUpdate(Framework framework)
    {
        if (Addon is null) return;
    
        onUpdateHook ??= AddonHook.Hook<Delegates.Addon.Update>(Addon, 41, Update);
        onUpdateHook?.Enable();
        
        Service.Framework.Update -= OnFrameworkUpdate;
    }

    public void Dispose()
    {
        contextMenu.OnOpenGameObjectContextMenu -= OpenGameObjectContextMenu;
        contextMenu.Dispose();
        
        onReceiveEventHook?.Dispose();
        onUpdateHook?.Dispose();
        
        Service.Framework.Update -= OnFrameworkUpdate;
    }

    private nint ReceiveEvent(AgentInterface* agent, nint rawData, AtkValue* args, uint argCount, ulong sender)
    {
        var result = onReceiveEventHook!.Original(agent, rawData, args, argCount, sender);
        
        Safety.ExecuteSafe(() =>
        {
            // Fallthrough switch to make logic more clear
            switch (args[0].Int)
            {
                // Preset Button is LeftClicked
                case 2:
                    
                // Window is closed    
                case -1 or -2:
                    
                // Preset is deleted or overwritten    
                case 0 when argCount is 5:
                    MemoryHelper.Instance.SaveMarkerData();
                    break;
            }
        });

        return result;
    }
    
    private byte Update(AtkUnitBase* addon)
    {
        var result = onUpdateHook!.Original(addon);
    
        Safety.ExecuteSafe(() =>
        {
            var baseNode = new BaseNode("FieldMarker");
    
            if (HoveredIndex != -1)
            {
                lastHoveredIndex = HoveredIndex;
            }
    
            if (Service.Configuration.FieldMarkerData.ContainsKey(Service.ClientState.TerritoryType))
            {
                foreach (var index in Enumerable.Range(0, 5))
                {
                    // target nodes are 21, 23, 25, 27, and 29
                    // The "update slot" buttons are between these indexes, we don't care about that button
                    var nodeIndex = (uint)(21 + index * 2);
                    var settingIndex = index + 5 * SelectedPage;
    
                    if (Service.Configuration.FieldMarkerData[Service.ClientState.TerritoryType].MarkerData[settingIndex] is { } markerData)
                    {
                        // If we have string data for this node, set it, if not, let the game write whatever it would normally write.
                        var textNode = baseNode.GetComponentNode(nodeIndex).GetNode<AtkTextNode>(5);
                        if (markerData.Name != string.Empty)
                        {
                            textNode->SetText($"{settingIndex + 1}. {markerData.Name}");
                        }
                    }
                    
                    // We have settings for this zone, but this index is null, so set the string to empty.
                    // this causes the game to show the subtle + marker
                    else
                    {
                        var textNode = baseNode.GetComponentNode(nodeIndex).GetNode<AtkTextNode>(5);
                        textNode->SetText(string.Empty);
                    }
                }
            }
        });
    
        return result;
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
        if (Service.Configuration.FieldMarkerData.ContainsKey(Service.ClientState.TerritoryType))
        {
            // Check that the preset we are modifying exists
            var settingIndex = lastHoveredIndex + 5 * SelectedPage;
            if (Service.Configuration.FieldMarkerData[Service.ClientState.TerritoryType].MarkerData[settingIndex] is not null)
            {
                RenameWindow.ShowWindow(settingIndex);
            }
        }
    }
}