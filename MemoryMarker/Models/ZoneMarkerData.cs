using System;
using System.Linq;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace MemoryMarker.DataModels;

public class NamedMarker
{
    public string Name = string.Empty;
    public FieldMarkerPreset Marker { get; set; }
}

public class ZoneMarkerData
{
    public NamedMarker?[] MarkerData { get; init; } = null!;
    public int GetMarkerCount()
    {
        return MarkerData.Where(marker => marker is not null).Count();
    }

    /// <summary>
    /// Updates MarkerData Array without changing Marker Names
    /// </summary>
    /// <param name="newData">Array of nullable NamedMarkers</param>
    public void UpdateMarkerData(NamedMarker?[] newData)
    {
        var maxSize = Math.Max(MarkerData.Length, newData.Length);

        foreach (var index in Enumerable.Range(0, maxSize))
        {
            var namedMarker = MarkerData[index];
            var newMarker = newData[index];

            // If both are not null, update stored marker
            if (namedMarker is not null && newMarker is not null)
            {
                namedMarker.Marker = newMarker.Marker;
            }

            // If we don't have a entry for this value, make one
            else if (namedMarker is null && newMarker is not null)
            {
                MarkerData[index] = new NamedMarker
                {
                    Marker = newMarker.Marker,
                    Name = string.Empty
                };
            }

            // If we do have an entry, but the new data is blank, clear our entry
            else if (namedMarker is not null && newMarker is null)
            {
                MarkerData[index] = null;
            }
        }
    }
}