using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.Textures.DataContainers;
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
