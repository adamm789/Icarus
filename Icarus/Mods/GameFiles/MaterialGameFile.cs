using Icarus.Mods.Interfaces;
using xivModdingFramework.Materials.DataContainers;

namespace Icarus.Mods.GameFiles
{
    public class MaterialGameFile : GameFile, IMaterialGameFile
    {
        public XivMtrl XivMtrl { get; set; }
        public int MaterialSet { get; set; }
        public string Variant { get; set; }
    }
}
