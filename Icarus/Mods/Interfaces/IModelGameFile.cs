using xivModdingFramework.General.Enums;
using xivModdingFramework.Models.DataContainers;

namespace Icarus.Mods.Interfaces
{
    public interface IModelGameFile : IGameFile
    {
        TTModel? TTModel { get; }
        XivMdl? XivMdl { get; }
        XivRace TargetRace { get; }
    }
}
