using xivModdingFramework.Materials.DataContainers;

namespace Icarus.Mods.Interfaces
{
    public interface IMaterialGameFile : IGameFile
    {
        XivMtrl XivMtrl { get; set; }
    }
}
