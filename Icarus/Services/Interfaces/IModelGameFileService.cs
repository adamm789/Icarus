using Icarus.Mods.GameFiles;
using ItemDatabase.Interfaces;
using System;
using xivModdingFramework.General.Enums;

namespace Icarus.Services.Interfaces
{
    public interface IModelGameFileService : IServiceProvider
    {
        ModelGameFile? GetModelFileData(string str);
        ModelGameFile? GetModelFileData(IItem? item = null, XivRace race = XivRace.Hyur_Midlander_Male);
    }
}
