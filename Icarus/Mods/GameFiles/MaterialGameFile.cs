using Icarus.Mods.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.Materials.DataContainers;

namespace Icarus.Mods.GameFiles
{
    public class MaterialGameFile : GameFile, IMaterialGameFile
    {
        public XivMtrl XivMtrl { get; set; }
    }
}
