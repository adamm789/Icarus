using Icarus.Mods.Interfaces;
using ItemDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.Mods.GameFiles
{
    public class GameFile : IGameFile
    {

        public string Path { get; set; } = "";
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
    }
}
