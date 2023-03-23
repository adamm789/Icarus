using Icarus.Mods.DataContainers;
using System.Collections.Generic;
using System.Windows.Documents;

namespace Icarus.Mods.Penumbra
{
    public class PenumbraMeta
    {
        public string Name = " My Penumbra Mod";
        public string Author = "Penumbra User";
        public string Version = "1.0.0";
        public string Description = "";
        public string Website = "";
        public int FileVersion = 1;
        public ulong ImportDate = 0;
        public List<string> ModTags = new();

        public PenumbraMeta()
        {

        }

        public PenumbraMeta(ModPack m)
        {
            Name = m.Name;
            Author = m.Author;
            Version = m.Version;
            Description = m.Description;
            Website = m.Url;
            ModTags = m.ModTags;
        }
    }
}
