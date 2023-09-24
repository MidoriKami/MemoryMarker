using System.Numerics;
using Dalamud.Interface.Internal.Notifications;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ImGuiNET;
using KamiLib;
using KamiLib.Utilities;
using MemoryMarker.Controllers;

namespace MemoryMarker.Windows;

public unsafe class RenameWindow : Window
{
    private static RenameWindow? _instance;

    private AgentFieldMarker* AgentFieldMarker => (AgentFieldMarker*) AgentModule.Instance()->GetAgentByInternalId(AgentId.FieldMarker);
    private int SelectedSlot => AgentFieldMarker->PageIndexOffset;
    
    private RenameWindow() : base("Rename Waymark")
    {
        IsOpen = true;

        Size = new Vector2(200.0f, 125.0f);
        SizeCondition = ImGuiCond.Always;

        Flags |= ImGuiWindowFlags.NoResize;
        Flags |= ImGuiWindowFlags.NoCollapse;
    }

    public static void ShowWindow()
    {
        if (_instance is null)
        {
            _instance = new RenameWindow();
            KamiCommon.WindowManager.AddWindow(_instance);
        }
    }

    public override void Draw()
    {
        if (MemoryMarkerSystem.Configuration.FieldMarkerData[Service.ClientState.TerritoryType].MarkerData[SelectedSlot] is not { } setting) return;

        if (setting is { Name: "" })
        {
            setting.Name = MemoryHelper.ReadSeString(AgentFieldMarker->PresetLabelsSpan.Get(SelectedSlot)).ToString()[3..];
        }

        ImGuiHelpers.ScaledDummy(10.0f);

        if (ImGui.IsWindowAppearing()) ImGui.SetKeyboardFocusHere();

        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
        if (ImGui.InputText("###RenameTextInput", ref setting.Name, 35, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
        {
            IsOpen = false;
        }

        var buttonSize = ImGuiHelpers.ScaledVector2(100.0f, 23.0f);
        ImGui.SetCursorPosX(ImGui.GetContentRegionMax().X - buttonSize.X);
        if (ImGui.Button("Save & Close", buttonSize))
        {
            IsOpen = false;
        }
    }

    public override void OnClose()
    {
        Service.PluginInterface.UiBuilder.AddNotification($"""Preset "{AgentFieldMarker->PresetLabelsSpan[SelectedSlot].ToString()[3..]}" saved.""", "Memory Marker", NotificationType.Success);

        KamiCommon.WindowManager.RemoveWindow(this);
        _instance = null;
        MemoryMarkerSystem.Configuration.Save();
    }
}