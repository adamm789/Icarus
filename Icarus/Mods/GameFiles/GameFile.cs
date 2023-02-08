using Icarus.Mods.Interfaces;
using ItemDatabase.Interfaces;

namespace Icarus.Mods.GameFiles
{
    public class GameFile : IGameFile
    {
        public virtual string Path { get; set; } = "";
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
    }
}
