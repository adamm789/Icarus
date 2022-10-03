using Icarus.Mods.DataContainers;
using Icarus.ViewModels.Mods;

namespace Icarus.Mods.Penumbra
{
    internal class PenumbraMeta
    {
        public string Name;
        public string Author;
        public string Version;
        public string Description;
        public string Url;
        public int FileVersion = 1;
        public ulong ImportDate = 0;

        public PenumbraMeta(ModPack m)
        {
            Name = m.Name;
            Author = m.Author;
            Version = m.Version;
            Description = m.Description;
            Url = m.Url;
        }
    }
}
