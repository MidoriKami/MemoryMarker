using System.Numerics;
using System.Runtime.InteropServices;
using KamiLib.Caching;
using Lumina.Excel.GeneratedSheets;

namespace MemoryMarker.DataModels;

[StructLayout( LayoutKind.Sequential, Pack = 0, Size = 104 )]
public struct FieldMarkerStruct
{
    public Vector3 A;
    public Vector3 B;
    public Vector3 C;
    public Vector3 D;
    public Vector3 One;
    public Vector3 Two;
    public Vector3 Three;
    public Vector3 Four;
    public byte ActiveMarkers;
    public byte Reserved;
    public ushort ContentFinderConditionId;
    public int Timestamp;

    public ContentFinderCondition? GetContentFinderCondition() => LuminaCache<ContentFinderCondition>.Instance.GetRow(ContentFinderConditionId);
    public uint GetTerritoryId() => GetContentFinderCondition()?.TerritoryType.Row ?? 0;
}