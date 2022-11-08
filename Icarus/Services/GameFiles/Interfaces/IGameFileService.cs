using Icarus.Mods;
using Icarus.Mods.Interfaces;
using ItemDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using xivModdingFramework.General.Enums;
using xivModdingFramework.Materials.FileTypes;

namespace Icarus.Services.GameFiles.Interfaces
{

    public interface IGameFileService : IServiceProvider
    {
        IItem? GetItem(IItem? itemArg = null);
        //Task<IGameFile?> GetFileData(IItem? itemArg = null, Type? type = null);
        /// <summary>
        /// Try to get game file associated with the given path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        //Task<IGameFile?> TryGetFileData(string path, Type? callingType = null, string name = "");

        /// <summary>
        /// Gets a list of races that have a unique mdl file of the given item
        /// </summary>
        /// <param name="itemArg"></param>
        /// <returns></returns>
        /*
        List<XivRace> GetAllRaceMdls(IItem? item = null);

        IModelGameFile? GetModelFileData(IItem? item = null, XivRace race = XivRace.Hyur_Midlander_Male);
        IModelGameFile? TryGetModelFileData(string path, string name = "");

        Task<IMaterialGameFile?> GetMaterialFileData(IItem? item);
        Task<IMaterialGameFile?> TryGetMaterialFileData(string path, string name = "");

        Task<ITextureGameFile?> GetTextureFileData(IItem? item = null);
        Task<ITextureGameFile?> TryGetTextureFileData(string path, string name = "");
        */
    }
}
