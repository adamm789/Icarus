using ItemDatabase.Enums;
using xivModdingFramework.Cache;
using xivModdingFramework.Textures.Enums;

namespace ItemDatabase.Interfaces
{
    // TODO: Face
    // TODO: Hair
    public interface IItem
    {
        public string Name { get; }
        public ulong ModelMain { get; }
        public MainItemCategory Category { get; }

        string GetMdlPath();
        string GetMdlFileName();
        string GetMtrlPath(string variant = "a");
        string GetMtrlFileName();
        string GetTexPath(XivTexType type, string variant = "");
        string GetMetadataPath();
        bool IsMatch(string str);
    }
}
