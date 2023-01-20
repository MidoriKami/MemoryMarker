using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using ImGuiNET;
using KamiLib;

namespace MemoryMarker.Windows;

public class RenameWindow : Window
{
    private static RenameWindow? _instance;

    private readonly int slotIndex;
    
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
            PluginLog.Debug($"Opening Rename Window: Slot {slotIndex + 1}");
        }
    }
    
    public override void Draw()
    {
        var setting = Service.Configuration.FieldMarkerData[Service.ClientState.TerritoryType].MarkerData[slotIndex];
        if (setting is null) return;

        if (setting.Name == string.Empty)
        {
            setting.Name = Service.AddonFieldMarker.GetTooltipFirstLine();
        }
        
        ImGuiHelpers.ScaledDummy(10.0f);

        var region = ImGui.GetContentRegionAvail();
        ImGui.SetNextItemWidth(region.X);
        ImGui.InputText("###RenameTextInput", ref setting.Name, 35, ImGuiInputTextFlags.AutoSelectAll);

        ImGuiHelpers.ScaledDummy(10.0f);
        ImGui.SetCursorPos(ImGui.GetCursorPos() with {X = region.X - 93.0f});
        if (ImGui.Button("Save & Close", ImGuiHelpers.ScaledVector2(100.0f, 23.0f)))
        {
            Service.Configuration.Save();
            IsOpen = false;
        }
    }

    public override void OnClose()
    {
        KamiCommon.WindowManager.RemoveWindow(this);
        _instance = null;
        Service.Configuration.Save();

        PluginLog.Debug($"Closing Rename Window: Slot {slotIndex + 1}");
    }
}