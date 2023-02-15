using System.Collections.Generic;

namespace Icarus.Mods.Penumbra
{
    internal class PenumbraDefaultMod
    {
        public string Name = "";
        public int Priority = 0;

        /// <summary>
        /// Key: game path
        /// Value: relative path on disk 
        /// </summary>
        public Dictionary<string, string> Files = new();
        public Dictionary<string, string> FileSwaps = new();
        // TODO: File swaps?
        List<PenumbraManipulation> Manipulations = new(); // TODO: meta manipulations
    }
}
