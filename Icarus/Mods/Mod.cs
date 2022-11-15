using Icarus.Mods.GameFiles;
using Icarus.Mods.Interfaces;
using Icarus.Util.Import;
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

        public bool IsDefault { get; set; } = false;
        public ImportSource ImportSource { get; set; }
        public bool ShouldImport { get; set; } = true;
        public bool ShouldExport { get; set; } = true;

        public Mod(ImportSource source)
        {
            ImportSource = source;
        }

        public Mod(IGameFile gameFile, ImportSource source)
        {
            ImportSource = source;
            if (source == ImportSource.Vanilla)
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
                ModFilePath = "Vanilla";
                //ModFilePath = gameFile.Path;
            }
            SetModData(gameFile);
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
