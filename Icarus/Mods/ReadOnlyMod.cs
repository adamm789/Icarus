using Icarus.Mods.Interfaces;

namespace Icarus.Mods
{
    public class ReadOnlyMod : Mod
    {
        public ReadOnlyMod() : base()
        {

        }
        public ReadOnlyMod(IGameFile file) : base(file)
        {

        }
        public byte[] Data;
    }
}
