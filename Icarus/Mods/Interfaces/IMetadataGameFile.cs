using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.Mods.FileTypes;

namespace Icarus.Mods.Interfaces
{
    public interface IMetadataFile : IGameFile
    {
        public ItemMetadata ItemMetadata { get; set; }
    }
}
