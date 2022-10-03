using ItemDatabase.Paths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.Materials.DataContainers;
using xivModdingFramework.Textures.DataContainers;
using xivModdingFramework.Textures.Enums;

namespace Icarus.Mods
{
    public class TextureMod : Mod
    {
        // TODO: What information do I need from a TextureMod?
        public XivTex XivTex { get; set; }
        public XivTexFormat TexFormat { get; set; } = XivTexFormat.INVALID;
        public XivTexType TexType { get; set; } = XivTexType.Normal;
        
        public TextureMod(XivTex tex)
        {
            XivTex = tex;
            TexFormat = tex.TextureFormat;
            TexType = tex.TextureTypeAndPath.Type;
        }

        public override bool IsComplete()
        {
            return !String.IsNullOrWhiteSpace(Path) && !String.IsNullOrWhiteSpace(ModFilePath);
        }
    }
}
