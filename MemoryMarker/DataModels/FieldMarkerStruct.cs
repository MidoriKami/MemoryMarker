using System.Runtime.InteropServices;
using KamiLib.Caching;
using Lumina.Excel.GeneratedSheets;

namespace MemoryMarker.DataModels;

// Based off of: https://github.com/PunishedPineapple/WaymarkPresetPlugin/blob/master/WaymarkPresetPlugin/GameStructs.cs
// For our purposes we don't need to actually use any of this data except for ContentFinderConditionID
// However, it has to be stored correctly or else we can't write it back without having to do extra effort.
[StructLayout(LayoutKind.Sequential, Pack = 0, Size = 12)]
public struct GamePresetPoint
{
    public int X;
    public int Y;
    public int Z;
}

[StructLayout( LayoutKind.Sequential, Pack = 0, Size = 104 )]
public struct FieldMarkerStruct
{
    public GamePresetPoint A;
    public GamePresetPoint B;
    public GamePresetPoint C;
    public GamePresetPoint D;
    public GamePresetPoint One;
    public GamePresetPoint Two;
    public GamePresetPoint Three;
    public GamePresetPoint Four;
    public byte ActiveMarkers;
    public byte Reserved;
    public ushort ContentFinderConditionId;
    public int Timestamp;

    private ContentFinderCondition? GetContentFinderCondition() => LuminaCache<ContentFinderCondition>.Instance.GetRow(ContentFinderConditionId);
    public uint GetTerritoryId() => GetContentFinderCondition()?.TerritoryType.Row ?? 0;
}