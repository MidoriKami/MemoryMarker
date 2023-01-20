using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using KamiLib.Interfaces;
using KamiLib.Windows;
using MemoryMarker.Utilities;

namespace MemoryMarker.Windows;

public class DebugWindow : SelectionWindow
{
    public DebugWindow() : base("Waypoint Debug Helper", 0.35f, 20.0f)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(800, 450),
            MaximumSize = new Vector2(9999, 9999)
        };
        
        IsOpen = true;

        Flags |= ImGuiWindowFlags.NoScrollbar;
        Flags |= ImGuiWindowFlags.NoScrollWithMouse;
    }
    
    protected override IEnumerable<ISelectable> GetSelectables()
    {
        return new List<ISelectable>();
    }

    protected override void DrawExtras()
    {
        PluginVersion.Instance.DrawVersionText();
    }
}