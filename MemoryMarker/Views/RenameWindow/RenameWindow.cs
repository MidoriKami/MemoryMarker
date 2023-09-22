using System.Numerics;
using System.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface;
using Dalamud.Interface.Internal.Notifications;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ImGuiNET;
using KamiLib;
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

        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(200, 125),
            MaximumSize = new Vector2(200, 125)
        };

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

        if (setting is { Name: not "" })
        {
            var name = AgentFieldMarker->PresetLabelsSpan[SelectedSlot].ToString();
            var filteredString = name[(name.IndexOf(' ') + 1)..];
            setting.Name = SeString.Parse(Encoding.UTF8.GetBytes(filteredString)).ToString();
        }

        ImGuiHelpers.ScaledDummy(10.0f);

        var region = ImGui.GetContentRegionAvail();

        if (ImGui.IsWindowAppearing())
        {
            ImGui.SetKeyboardFocusHere();
        }

        ImGui.SetNextItemWidth(region.X);
        if (ImGui.InputText("###RenameTextInput", ref setting.Name, 35, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
        {
            MemoryMarkerSystem.Configuration.Save();
            IsOpen = false;
        }

        var buttonSize = ImGuiHelpers.ScaledVector2(100.0f, 23.0f);
        ImGui.SetCursorPosX(ImGui.GetContentRegionMax().X - buttonSize.X);
        if (ImGui.Button("Save & Close", buttonSize))
        {
            MemoryMarkerSystem.Configuration.Save();
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