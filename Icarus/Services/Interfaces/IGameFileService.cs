﻿using Icarus.Mods.GameFiles;
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
        Task<IGameFile?> GetFileData(IItem? itemArg = null, Type? type = null);
        /// <summary>
        /// Try to get game file associated with the given path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<IGameFile?> TryGetFileData(string path);

        /// <summary>
        /// Gets a list of races that have a unique mdl file of the given item
        /// </summary>
        /// <param name="itemArg"></param>
        /// <returns></returns>
        List<XivRace> GetAllRaceMdls(IItem? item = null);


        ModelGameFile? GetModelFileData(IItem? item = null, XivRace race = XivRace.Hyur_Midlander_Male);

        ModelGameFile? GetModelFileData(string str);
        Task<MaterialGameFile?> GetMaterialFileData(IItem? item);

        Task<MaterialGameFile?> GetMaterialFileData(string str);

        public XivTexFormat GetTextureData(IItem? itemArg = null);
    }
}
