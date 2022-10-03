using Icarus.Mods.GameFiles;
using Icarus.Mods.Interfaces;
using ItemDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.General.Enums;
using xivModdingFramework.Textures.Enums;

namespace Icarus.Services.Interfaces
{
    public interface IGameFileService : IServiceProvider
    {
        ModelGameFile? GetModelFileData(string str);
        ModelGameFile? GetModelFileData(IItem? item = null, XivRace race = XivRace.Hyur_Midlander_Male);
        Task<MaterialGameFile?> GetMaterialFileData(string str);
        Task<MaterialGameFile?> GetMaterialFileData(IItem? item);

        public XivTexFormat GetTextureData(IItem? itemArg = null);

        /// <summary>
        /// Try to get game file associated with the given path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="race"></param>
        /// <returns></returns>
        Task<IGameFile?> TryGetFileData(string path, XivRace race = XivRace.Hyur_Midlander_Male);

        /// <summary>
        /// Gets a list of races that have a unique mdl file of the given item
        /// </summary>
        /// <param name="itemArg"></param>
        /// <returns></returns>
        List<XivRace> GetAllRaceMdls(IItem? item = null);
    }
}
