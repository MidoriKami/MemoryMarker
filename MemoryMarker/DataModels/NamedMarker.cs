namespace MemoryMarker.DataModels;

public class NamedMarker
{
    public FieldMarkerStruct Marker { get; init; }
    public string Name { get; set; } = string.Empty;
}