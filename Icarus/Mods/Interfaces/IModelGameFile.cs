using Icarus.Services.Interfaces;
using Icarus.ViewModels.Util;
using ItemDatabase.Enums;
using ItemDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
