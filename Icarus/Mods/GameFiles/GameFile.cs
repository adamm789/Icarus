using Icarus.Mods.Interfaces;

namespace Icarus.Mods.GameFiles
{
    public class GameFile : IGameFile
    {

        public string Path { get; set; } = "";
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
    }
}
