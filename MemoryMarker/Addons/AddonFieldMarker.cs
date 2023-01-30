using System;
using System.Linq;
using Dalamud.ContextMenu;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiLib.Atk;
using KamiLib.Hooking;
using MemoryMarker.Utilities;
using MemoryMarker.Windows;

namespace MemoryMarker.Addons;

public unsafe class AddonFieldMarker : IDisposable
{
    private readonly DalamudContextMenu contextMenu;
    private readonly GameObjectContextMenuItem contextMenuItem;
    private static SeString ContextMenuLabel => new(new TextPayload("Rename"));

    [Signature("40 53 48 83 EC 50 F6 81 ?? ?? ?? ?? ?? 48 8B D9 0F 29 74 24 ?? 0F 28 F1 74 7B", DetourName = nameof(Update))]
    private readonly Hook<Delegates.Addon.Update>? onUpdateHook = null!;

    private readonly Hook<Delegates.Agent.ReceiveEvent>? onReceiveEventHook;

    private static AtkUnitBase* AddonBase => (AtkUnitBase*) Service.GameGui.GetAddonByName("FieldMarker");
    private byte SelectedPage => *((byte*)AddonBase + 1408);
    private int HoveredIndex => *(int*) ((byte*) AddonBase + 1404);
    private int lastHoveredIndex;
    
    public AddonFieldMarker()
    {
        SignatureHelper.Initialise(this);
        
        contextMenu = new DalamudContextMenu();
        contextMenuItem = new GameObjectContextMenuItem(ContextMenuLabel, RenameContextMenuAction, true);
        contextMenu.OnOpenGameObjectContextMenu += OpenGameObjectContextMenu;

        var agent = AgentModule.Instance()->GetAgentByInternalId(AgentId.FieldMarker);

        onReceiveEventHook ??= Hook<Delegates.Agent.ReceiveEvent>.FromAddress(new nint(agent->VTable->ReceiveEvent), ReceiveEvent);
        onReceiveEventHook.Enable();
        
        onUpdateHook.Enable();
    }

    public void Dispose()
    {
        contextMenu.OnOpenGameObjectContextMenu -= OpenGameObjectContextMenu;
        contextMenu.Dispose();
        
        onReceiveEventHook?.Dispose();
        onUpdateHook?.Dispose();
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
    
    private string? GetTooltipText()
    {
        var fieldMarkerAgent = AgentModule.Instance()->GetAgentByInternalId(AgentId.FieldMarker);
        var tooltipStringPointer = (Utf8String*)((byte*) fieldMarkerAgent + 3184);
        if (tooltipStringPointer->IsEmpty != 0) return null;

        return tooltipStringPointer->ToString();
    }

    public string GetTooltipFirstLine()
    {
        var fullTooltip = GetTooltipText();
        if (fullTooltip is null) throw new Exception("Something went wrong, this should only get called when renaming a valid preset.");

        var splits = fullTooltip.Split("\n");
        
        var firstLine = splits.FirstOrDefault();
        if (firstLine is null) throw new FormatException("Failed to parse tooltip text.");

        var firstSpaceIndex = firstLine.IndexOf(" ", StringComparison.Ordinal) + 1;

        return firstLine[firstSpaceIndex..];
    }
}