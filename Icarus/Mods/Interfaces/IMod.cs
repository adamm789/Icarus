using Icarus.Util.Import;
using System;

namespace Icarus.Mods.Interfaces
{
    public interface IMod : IGameFile
    {
        string ModFileName { get; }

        /// <summary>
        /// Path to the user-provided file
        /// </summary>
        string ModFilePath { get; }
        bool IsDefault { get; set; }
        ImportSource ImportSource { get; }

        void SetModData(IGameFile gameFile);

        bool IsComplete();
        string ToVerboseString();
    }
}
