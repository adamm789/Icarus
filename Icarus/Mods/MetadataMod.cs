using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.Mods.FileTypes;

namespace Icarus.Mods
{
    public class MetadataMod : Mod
    {
        ItemMetadata _data;
        public MetadataMod(ItemMetadata data)
        {
            _data = data;
        }
    }
}
