using KamiLib.Window;

namespace MemoryMarker;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
public static class System {
	public static Configuration Configuration { get; set; }
	public static AddonFieldMarkerContextMenu ContextMenu { get; set; }
	public static AddonFieldMarkerController FieldMarkerController { get; set; }
	public static WindowManager WindowManager { get; set; }
}