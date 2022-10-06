using Icarus.Mods.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.Textures.DataContainers;
using xivModdingFramework.Textures.Enums;

namespace Icarus.Mods.GameFiles
{
    public class TextureGameFile : GameFile, ITextureGameFile
    {
        public XivTex? XivTex { get; set; }
        public XivTexType TexType { get; set; } = XivTexType.Other;
        public XivTexFormat TexFormat { get; set; } = XivTexFormat.INVALID;
        public Dictionary<XivTexType, XivTexFormat> TypeFormatDict { get; set; }
    }
}
