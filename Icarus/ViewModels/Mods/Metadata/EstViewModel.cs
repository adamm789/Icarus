using Icarus.ViewModels.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.General.Enums;
using xivModdingFramework.Models.DataContainers;

namespace Icarus.ViewModels.Mods.Metadata
{
    public class EstViewModel : NotifyPropertyChanged
    {
        Dictionary<XivRace, ExtraSkeletonEntry> _entries;

        public EstViewModel(Dictionary<XivRace, ExtraSkeletonEntry> dict)
        {

        }
    }
}
