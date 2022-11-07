using System.Collections.Generic;
using xivModdingFramework.Materials.DataContainers;
using xivModdingFramework.Textures.DataContainers;
using xivModdingFramework.Textures.Enums;

namespace Icarus.Mods.Interfaces
{
    public interface ITextureGameFile : IGameFile
    {
        public XivMtrl? XivMtrl { get; set; }
        public XivTex? XivTex { get; set; }
        XivTexType TexType { get; set; }
        //XivTexFormat TexFormat { get; set; }
        public Dictionary<XivTexType, XivTexFormat> TypeFormatDict { get; set; }

    }
}
