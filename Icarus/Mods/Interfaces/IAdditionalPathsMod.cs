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

        /// <summary>
        /// Dictionary where key is the path and value is the filename
        /// </summary>
        Dictionary<string, string> AllPathsDictionary { get; }
    }
}
