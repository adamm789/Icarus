using System.Collections.Generic;
using xivModdingFramework.Textures.Enums;

namespace Icarus.Mods.Interfaces
{
    public interface ITextureGameFile : IGameFile
    {
        XivTexType TexType { get; set; }
        //XivTexFormat TexFormat { get; set; }
        public Dictionary<XivTexType, XivTexFormat> TypeFormatDict { get; set; }

    }
}
