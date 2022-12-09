using System.Collections.Generic;
using System.Windows.Documents;
using xivModdingFramework.Materials.DataContainers;
using xivModdingFramework.Textures.DataContainers;
using xivModdingFramework.Textures.Enums;

namespace Icarus.Mods.Interfaces
{
    public interface IMaterialGameFile : IGameFile
    {
        XivMtrl XivMtrl { get; set; }
        int MaterialSet { get; set; }
        string Variant { get; set; }
    }
}
