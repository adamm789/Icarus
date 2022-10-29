using Icarus.Mods.Interfaces;
using Icarus.Util.Import;

namespace Icarus.Mods
{
    public class ReadOnlyMod : Mod
    {
        public ReadOnlyMod(ImportSource source = ImportSource.TexToolsModPack) : base(source)
        {

        }

        public override void SetModData(IGameFile gameFile)
        {
            
        }

        public byte[] Data;
    }
}
