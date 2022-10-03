using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.Mods.Penumbra
{
    public class PenumbraModGroup
    {
        public string Name = "";
        public string Description = "";
        public int Priority = 0;
        public string Type = "Single";
        public List<PenumbraModOption> Options = new();
    }
}
