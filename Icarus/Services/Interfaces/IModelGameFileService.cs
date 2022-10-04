using Icarus.Mods.GameFiles;
using ItemDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.General.Enums;

namespace Icarus.Services.Interfaces
{
    public interface IModelGameFileService : IServiceProvider
    {
        ModelGameFile? GetModelFileData(string str);
        ModelGameFile? GetModelFileData(IItem? item = null, XivRace race = XivRace.Hyur_Midlander_Male);
    }
}
