using System;
using Dalamud.Interface.GameFonts;

namespace MemoryMarker.System;

public class FontManager : IDisposable
{
    public GameFontHandle Axis12 { get; }

    public FontManager()
    {
        Axis12 = Service.PluginInterface.UiBuilder.GetGameFontHandle( new GameFontStyle(GameFontFamilyAndSize.Axis12) );
    }

    public void Dispose()
    {
        Axis12.Dispose();
    }
}