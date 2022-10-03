﻿using ItemDatabase.Interfaces;
using Icarus.ViewModels.Import;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.Mods.DataContainers;
using Icarus.Services.Interfaces;
using Icarus.Mods.Interfaces;
using Icarus.Mods.GameFiles;

namespace Icarus.Mods
{
    /// <summary>
    /// Abstract implementation of a mod.
    /// Namely a file that replaces a game file
    /// </summary>
    public abstract class Mod : GameFile, IMod
    {
        public string ModFileName { get; set; } = "";
        public string ModFilePath { get; set; } = "";

        public bool IsInternal { get; set; } = false;
        public Mod()
        {
            
        }

        public Mod(IGameFile gameFile, bool isInternal = false)
        {
            IsInternal = isInternal;
            SetModData(gameFile);
        }

        public virtual bool IsComplete()
        {
            return true;
        }

        public virtual void SetModData(IGameFile gameFile)
        {
            ModFileName = gameFile.Name;
            ModFilePath = gameFile.Path;

            Path = gameFile.Path;
            Name = gameFile.Name;
            Category = gameFile.Category;
        }

        public virtual string ToVerboseString()
        {
            return 
                $"[ModFileName]: {ModFileName}\n" +
                $"[ModFilePath]: {ModFilePath}\n" +
                $"[Name]       : {Name}\n" +
                $"[Path]       : {Path}\n" +
                $"[Category]   : {Category}";
        }
    }
}