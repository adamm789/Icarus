using Icarus.Mods.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.Mods
{
    public class ReadOnlyMod : Mod
    {
        public ReadOnlyMod() :base()
        {

        }
        public ReadOnlyMod(IGameFile file) : base(file)
        {

        }
        public byte[] Data;
    }
}
