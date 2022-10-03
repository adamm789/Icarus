using System.Collections.Generic;
using xivModdingFramework.Mods.DataContainers;

namespace Icarus.Old
{
    public class ModGroup
    {
        private ModGroupJson ModGroupJson = new();

        public string GroupName
        {
            get { return ModGroupJson.GroupName; }
            set { ModGroupJson.GroupName = value; }
        }
        public string SelectionType
        {
            get { return ModGroupJson.SelectionType; }
            set { ModGroupJson.SelectionType = value; }
        }
        public List<ModOption> OptionList = new();
    }
}
