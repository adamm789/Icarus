using Icarus.Penumbra.GameData;
using System.Collections.Generic;

namespace Icarus.Mods.Penumbra
{
    public class PenumbraModOption
    {
        public string Name = "";
        public string Description = "";
        public Dictionary<string, string> Files = new();
        // FileSwaps
        //public List<PenumbraManipulation> Manipulations = new();
        public List<MetaManipulationContainer> Manipulations = new();
    }
}
