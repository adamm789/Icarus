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
        string GetMetadataPath();
        bool IsMatch(string str);
    }
}
