using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.Mods.Interfaces
{
    public interface IAdditionalPathsMod : IMod
    {
        bool AssignToAllPaths { get; }
        Dictionary<string, string> AllPathsDictionary { get; }
    }
}
