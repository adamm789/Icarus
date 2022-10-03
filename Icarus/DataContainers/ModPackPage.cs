using System.Collections.Generic;
using xivModdingFramework.Mods.DataContainers;

namespace Icarus.Old { 
    public class ModPackPage
    {
        private ModPackPageJson ModPackPageJson = new();

        public int PageIndex
        {
            get { return ModPackPageJson.PageIndex; }
            set { ModPackPageJson.PageIndex = value; }
        }
        public List<ModGroup> ModGroups = new();
    }
}
