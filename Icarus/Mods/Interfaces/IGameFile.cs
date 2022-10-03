using ItemDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.Mods.Interfaces
{
    public interface IGameFile
    {
        //public IItem? Item { get; set; }

        /// <summary>
        /// Name of the item in-game
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Path to the file in-game
        /// </summary>
        string Path { get; set; }

        string Category { get; set; }
    }
}
