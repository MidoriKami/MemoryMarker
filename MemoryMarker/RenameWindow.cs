using System.Numerics;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Interface.Internal.Notifications;
using Dalamud.Interface.Utility;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.Interop;
using ImGuiNET;
using KamiLib.Window;

namespace MemoryMarker;

public unsafe class RenameWindow : Window {
    private static RenameWindow? _instance;

    private RenameWindow() : base("Rename Waymark", new Vector2(200.0f, 125.0f), true) {
        IsOpen = true;

        AdditionalInfoTooltip = "Renames Waymark Presets";

        Flags |= ImGuiWindowFlags.NoResize;
        Flags |= ImGuiWindowFlags.NoCollapse;
    }

    private AgentFieldMarker* AgentFieldMarker => (AgentFieldMarker*) AgentModule.Instance()->GetAgentByInternalId(AgentId.FieldMarker);
    private int SelectedSlot => AgentFieldMarker->PageIndexOffset;
    private string SelectedSlotString => MemoryHelper.ReadSeString(AgentFieldMarker->PresetLabelsSpan.GetPointer(SelectedSlot)).ToString()[3..];

    public static void ShowWindow() {
        if (_instance is null) {
            MemoryMarkerSystem.WindowManager.AddWindow(_instance = new RenameWindow());
        }
    }

    public override void Draw() {
        if (MemoryMarkerSystem.Configuration.FieldMarkerData[Service.ClientState.TerritoryType].MarkerData[SelectedSlot] is not { } setting) return;

        if (setting is { Name: "" }) setting.Name = SelectedSlotString;

        ImGuiHelpers.ScaledDummy(10.0f);

        if (ImGui.IsWindowAppearing()) ImGui.SetKeyboardFocusHere();

        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
        if (ImGui.InputText("###RenameTextInput", ref setting.Name, 35, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue)) {
            IsOpen = false;
        }

        var buttonSize = ImGuiHelpers.ScaledVector2(100.0f, 23.0f);
        ImGui.SetCursorPosX(ImGui.GetContentRegionMax().X - buttonSize.X);
        if (ImGui.Button("Save & Close", buttonSize)) {
            IsOpen = false;
        }
    }

    public override void OnClose() {
        if (MemoryMarkerSystem.Configuration.FieldMarkerData[Service.ClientState.TerritoryType].MarkerData[SelectedSlot] is not { } setting) return;

        if (setting is { Name: "" }) setting.Name = SelectedSlotString;

        Service.NotificationManager.AddNotification(new Notification {
            Type = NotificationType.Success,
            Content = $"""Preset "{setting.Name}" saved.""",
            Title = "Memory Marker"
        });

        MemoryMarkerSystem.WindowManager.RemoveWindow(this);
        _instance = null;
        MemoryMarkerSystem.Configuration.Save();
    }
}