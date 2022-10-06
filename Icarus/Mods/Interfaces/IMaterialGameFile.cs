using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.Materials.DataContainers;

namespace Icarus.Mods.Interfaces
{
    public interface IMaterialGameFile : IGameFile
    {
        XivMtrl XivMtrl { get; set; }
    }
}
