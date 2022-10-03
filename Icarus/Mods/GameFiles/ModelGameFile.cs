using Icarus.Mods.Interfaces;
using ItemDatabase.Enums;
using ItemDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.General.Enums;
using xivModdingFramework.Models.DataContainers;

namespace Icarus.Mods.GameFiles
{
    /// <summary>
    /// Contains the necessary information needed from an XIV model file
    /// </summary>
    public class ModelGameFile : GameFile, IModelGameFile
    {
        public TTModel? TTModel { get; set; }
        public XivMdl? XivMdl { get; set; }
        public XivRace TargetRace { get; set; } = XivRace.Hyur_Midlander_Male;
    }
}
