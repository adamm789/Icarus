using System;

namespace Icarus.ViewModels.Mods.DataContainers
{
    [Flags]
    public enum ModPackViewModelImportFlags
    {
        AddMods = 0,
        OverwriteData = 1,

        OverwritePages = 2,
        AppendPagesToEnd = 4,
        AppendPagesToStart = 8   // TODO: Implement AppendToStart, maybe?
    }
}
