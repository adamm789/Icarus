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
        ExtraSkeletonEntry _est;
        public XivRace Race
        {
            get { return _est.Race; }
        }
        public ushort SetId
        {
            get { return _est.SetId; }
            set { _est.SetId = value; OnPropertyChanged(); } 
        }
        public ushort SkelId
        {
            get { return _est.SkelId; }
            set { _est.SkelId = value; OnPropertyChanged(); }
        }

        bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { _isEnabled = value; OnPropertyChanged(); }
        }
        public EstEntryViewModel(XivRace race, ExtraSkeletonEntry est)
        {
            _est = est;

            IsEnabled = _est.SkelId != 0;
        }
    }
}
