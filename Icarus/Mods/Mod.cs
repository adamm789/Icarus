using Icarus.Mods.GameFiles;
using Icarus.Mods.Interfaces;
using System;
using SystemPath = System.IO.Path;

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

            if (isInternal)
            {
                string fileName;
                try
                {
                    fileName = SystemPath.GetFileName(gameFile.Path);
                }
                catch (ArgumentException)
                {
                    fileName = gameFile.Path;
                }
                ModFileName = $"{fileName} ({gameFile.Name})";
                ModFilePath = gameFile.Path;
            }

            SetModData(gameFile);
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public virtual bool IsComplete()
        {
            return true;
        }

        public virtual void SetModData(IGameFile gameFile)
        {
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
