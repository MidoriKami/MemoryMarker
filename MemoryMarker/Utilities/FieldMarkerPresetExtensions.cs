using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using KamiLib.Caching;
using Lumina.Excel.GeneratedSheets;

namespace MemoryMarker.Utilities;

public static class FieldMarkerPresetExtensions
{
    public static ContentFinderCondition? GetContentFinderCondition(this FieldMarkerPreset data) => LuminaCache<ContentFinderCondition>.Instance.GetRow(data.ContentFinderConditionId);
    public static uint GetTerritoryId(this FieldMarkerPreset data) => GetContentFinderCondition(data)?.TerritoryType.Row ?? 0;
}
