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
