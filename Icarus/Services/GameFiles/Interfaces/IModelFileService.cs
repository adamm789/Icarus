using Icarus.Mods.Interfaces;
using ItemDatabase.Interfaces;
using System;
using System.Collections.Generic;
using xivModdingFramework.General.Enums;

namespace Icarus.Services.GameFiles.Interfaces
{
    public interface IModelFileService : IGameFileService
    {
        List<XivRace> GetAllRaceMdls(IItem? item = null);
        Dictionary<XivRace, IModelGameFile> GetModelFileDataDict(IItem? itemArg = null);

        IModelGameFile? GetModelFileData(IItem? item = null);
        IModelGameFile? GetModelFileData(IItem? item = null, XivRace race = XivRace.Hyur_Midlander_Male);
        IModelGameFile? TryGetModelFileData(string path, string name = "");
        List<IGear>? GetSharedModels(IGear gear);
        IModelGameFile? SelectedModelFile { get; set; }
    }
}
