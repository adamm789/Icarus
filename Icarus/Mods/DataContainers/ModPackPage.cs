using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.Mods.DataContainers;

namespace Icarus.Mods.DataContainers
{
    public class ModPackPage
    {
        public ModPackPage(int index)
        {
            PageIndex = index;
        }
        public ModPackPage(ModPackPageJson page)
        {
            PageIndex = page.PageIndex;
        }

        public ModPackPage(ModPackPage page)
        {
            PageIndex = page.PageIndex;
        }
        public void AddGroup(ModGroup group)
        {
            ModGroups.Add(group);
        }

        /// <summary>
        /// The page index starting at 1
        /// </summary>
        public int PageIndex { get; set; }

        public List<ModGroup> ModGroups = new();
    }
}
