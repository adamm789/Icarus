using Icarus.Util.Import;
using System;

namespace Icarus.Mods.Interfaces
{
    public interface IMod : IGameFile
    {
        string ModFileName { get; set; }

        /// <summary>
        /// Path to the user-provided file
        /// </summary>
        string ModFilePath { get; set; }
        bool IsDefault { get; set; }
        ImportSource ImportSource { get; }

        void SetModData(IGameFile gameFile);

        bool IsComplete();
        string ToVerboseString();
        bool ShouldImport { get; set; }
        bool ShouldExport { get; set; }
        IMod DeepCopy(IMod other);
    }
}
