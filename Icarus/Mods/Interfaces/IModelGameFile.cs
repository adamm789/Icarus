using xivModdingFramework.General.Enums;
using xivModdingFramework.Models.DataContainers;

namespace Icarus.Mods.Interfaces
{
    public interface IModelGameFile : IGameFile
    {
        /// <summary>
        /// TTModel of the Vanilla model
        /// </summary>
        TTModel? TTModel { get; }

        /// <summary>
        /// XivMdl of the Vanilla model
        /// </summary>
        XivMdl? XivMdl { get; }
        XivRace TargetRace { get; }
    }
}
