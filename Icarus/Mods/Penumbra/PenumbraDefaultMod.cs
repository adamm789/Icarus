using System.Collections.Generic;

namespace Icarus.Mods.Penumbra
{
    internal class PenumbraDefaultMod
    {
        public string Name = "";
        public int Priority = 0;
        public Dictionary<string, string> Files = new();
        // TODO: File swaps?
        List<PenumbraManipulations> Manipulations = new(); // TODO: meta manipulations
    }
}
