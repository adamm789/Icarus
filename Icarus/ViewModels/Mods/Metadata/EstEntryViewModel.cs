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
    public class EstEntryViewModel : NotifyPropertyChanged
    {
        public XivRace Race { get; }
        public ushort SetId { get; }
        public ushort SkelId { get; }

        public bool IsEnabled { get; }
        public EstEntryViewModel(XivRace race, ExtraSkeletonEntry est)
        {
            Race = race;
            SetId = est.SetId;
            SkelId = est.SkelId;

            IsEnabled = est.SkelId != 0;
        }
    }
}
