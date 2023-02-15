using System.Collections.Generic;

namespace Icarus.Mods.Penumbra
{
    public class PenumbraModGroup
    {
        public string Name = "";
        public string Description = "";
        public int Priority = 0;
        public string Type = "Single";
        public int DefaultSettings = 0;
        public List<PenumbraModOption> Options = new();
    }
}
