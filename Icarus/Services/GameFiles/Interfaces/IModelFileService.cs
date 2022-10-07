using Icarus.Mods.Interfaces;
using ItemDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.General.Enums;

namespace Icarus.Services.GameFiles.Interfaces
{
    public interface IModelFileService : IServiceProvider
    {
        List<XivRace> GetAllRaceMdls(IItem? item = null);
        IModelGameFile? GetModelFileData(IItem? item = null, XivRace race = XivRace.Hyur_Midlander_Male);
        IModelGameFile? TryGetModelFileData(string path, string name = "");
    }
}
