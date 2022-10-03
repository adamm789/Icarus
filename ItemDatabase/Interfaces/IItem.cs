using ItemDatabase.Enums;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        string GetMtrlPath();
        string GetMtrlFileName();
        string GetTexPath(XivTexType type);
        bool IsMatch(string str);
    }
}
