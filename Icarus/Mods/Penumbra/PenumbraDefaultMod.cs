using System.Collections.Generic;

namespace Icarus.Mods.Penumbra
{
    internal class PenumbraDefaultMod
    {
        public string Name = "";
        public int Priority = 0;

        /// <summary>
        /// Key: game path
        /// Value: path on disk 
        /// </summary>
        public Dictionary<string, string> Files = new();
        // TODO: File swaps?
        List<PenumbraManipulations> Manipulations = new(); // TODO: meta manipulations
    }
}
