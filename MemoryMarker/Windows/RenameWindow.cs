using System.Numerics;
using System.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface;
using Dalamud.Interface.Internal.Notifications;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ImGuiNET;
using KamiLib;

namespace MemoryMarker.Windows;

public unsafe class RenameWindow : Window
{
    private static RenameWindow? _instance;
    private AgentFieldMarker* AgentFieldMarker => (AgentFieldMarker*) AgentModule.Instance()->GetAgentByInternalId(AgentId.FieldMarker);

    private readonly int slotIndex;

    private bool isFocusSet;
    
    private RenameWindow(int slotIndex) : base("Rename Waymark")
    {
        this.slotIndex = slotIndex;
        IsOpen = true;

        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(200, 125),
            MaximumSize = new Vector2(200, 125),
        };

        Flags |= ImGuiWindowFlags.NoResize;
        Flags |= ImGuiWindowFlags.NoCollapse;
    }
    
    public static void ShowWindow(int slotIndex)
    {
        if (_instance is null)
        {
            _instance = new RenameWindow(slotIndex);
            KamiCommon.WindowManager.AddWindow(_instance);
        }
    }
    
    public override void Draw()
    {
        var setting = Service.Configuration.FieldMarkerData[Service.ClientState.TerritoryType].MarkerData[slotIndex];
        if (setting is null) return;

        if (setting.Name == string.Empty)
        {
            var name = AgentFieldMarker->PresetLabelsSpan[slotIndex].ToString();
            var filteredString = name[(name.IndexOf(' ') + 1)..];
            setting.Name = SeString.Parse(Encoding.UTF8.GetBytes(filteredString)).ToString();
        }
        
        ImGuiHelpers.ScaledDummy(10.0f);

        var region = ImGui.GetContentRegionAvail();
        SetFocus();
        ImGui.SetNextItemWidth(region.X);
        ImGui.PushFont(Service.FontManager.Axis12.ImFont);
        if (ImGui.InputText("###RenameTextInput", ref setting.Name, 35, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
        {
            Service.Configuration.Save();
            IsOpen = false;
        }
        ImGui.PopFont();

        ImGuiHelpers.ScaledDummy(10.0f);
        var buttonSize = ImGuiHelpers.ScaledVector2(100.0f, 23.0f);
        ImGui.SetCursorPos(ImGui.GetCursorPos() with {X = region.X - (buttonSize.X * 0.93f)});
        if (ImGui.Button("Save & Close", buttonSize))
        {
            Service.Configuration.Save();
            IsOpen = false;
        }
    }
    private void SetFocus()
    {
        if (!isFocusSet)
        {
            ImGui.SetKeyboardFocusHere();
            isFocusSet = true;
        }
    }

    public override void OnClose()
    {
        Service.PluginInterface.UiBuilder.AddNotification("Preset name saved.", "Memory Marker", NotificationType.Success);
        
        KamiCommon.WindowManager.RemoveWindow(this);
        _instance = null;
        Service.Configuration.Save();
    }
}