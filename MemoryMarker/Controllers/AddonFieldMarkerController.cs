using System;
using System.Linq;
using Dalamud.Game.Addon;
using FFXIVClientStructs.FFXIV.Client.UI;
using MemoryMarker.DataModels;
using MemoryMarker.Utilities;
using ValueType = FFXIVClientStructs.FFXIV.Component.GUI.ValueType;

namespace MemoryMarker.Controllers;

public unsafe class AddonFieldMarkerController : IDisposable
{
    public AddonFieldMarkerController()
    {
        Service.AddonLifecycle.RegisterListener(AddonEvent.PreDraw, "FieldMarker", OnPreDraw);
        Service.AddonLifecycle.RegisterListener(AddonEvent.PostUpdate, "FieldMarker", OnPostUpdate);
    }

    public void Dispose()
    {
        Service.AddonLifecycle.UnregisterListener(OnPreDraw, OnPostUpdate);
    }

    private void OnPostUpdate(AddonEvent eventType, IAddonArgs args)
    {
        var addon = (AddonFieldMarker*) args.Addon;
        if (!MemoryMarkerSystem.Configuration.FieldMarkerData.TryGetValue(Service.ClientState.TerritoryType, out var value)) return;

        var configChanged = false;

        foreach (var index in Enumerable.Range(0, value.MarkerData.Length))
        {
            var atkValueIndex = index * 2 + 34;
            ref var flagValue = ref addon->AtkUnitBase.AtkValues[atkValueIndex];
            ref var markerData = ref value.MarkerData[index];

            // There is a valid entry in this slot
            if (flagValue is { Type: ValueType.UInt, UInt: not 0 })
            {
                // Newly added
                if (markerData is null)
                {
                    Service.Log.Verbose($"Adding Node {index}");

                    markerData = new NamedMarker
                    {
                        Marker = MemoryHelper.GetPresetForIndex(index),
                        Name = string.Empty
                    };

                    configChanged = true;
                }
            }

            // There is no valid entry in this slot
            else
            {
                // Recently removed
                if (markerData is not null)
                {
                    Service.Log.Verbose($"Removing Node {index}");

                    markerData = null;
                    configChanged = true;
                }
            }
        }

        if (configChanged)
        {
            MemoryMarkerSystem.Configuration.Save();
        }
    }

    private void OnPreDraw(AddonEvent eventType, IAddonArgs args)
    {
        var addon = (AddonFieldMarker*) args.Addon;
        if (!MemoryMarkerSystem.Configuration.FieldMarkerData.TryGetValue(Service.ClientState.TerritoryType, out var value)) return;

        foreach (var index in Enumerable.Range(0, 5))
        {
            var entryIndex = index + addon->SelectedPage * 5;
            if (value.MarkerData[entryIndex] is not { Name: { Length: not 0 } name }) continue;

            var nodeId = (uint) (21 + index * 2);

            var buttonNode = addon->AtkUnitBase.GetButtonNodeById(nodeId);
            if (buttonNode is null) continue;

            var desiredLabel = $"{entryIndex + 1}. {name}";

            if (buttonNode->ButtonTextNode->NodeText.ToString() != desiredLabel)
            {
                buttonNode->ButtonTextNode->NodeText.SetString(desiredLabel);
            }
        }
    }
}