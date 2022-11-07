using Icarus.Mods.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.Mods.FileTypes;

namespace Icarus.Mods.GameFiles
{
    public class MetadataFile : GameFile, IMetadataFile
    {
        public ItemMetadata ItemMetadata { get; set; }
    }
}
