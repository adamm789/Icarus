using Icarus.Mods;
using Icarus.ViewModels.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.General.Enums;
using xivModdingFramework.Models.DataContainers;

namespace Icarus.ViewModels.Mods.Metadata
{
    public class EstEntryViewModel : NotifyPropertyChanged
    {
        // TODO: ComboBox of all available EstEntries...

        ExtraSkeletonEntry _est;
        EqdpEntryViewModel _eqdpViewModel;
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

        public EstEntryViewModel(XivRace race, MetadataMod mod, EqdpEntryViewModel eqdpViewModel)
        {
            _est = mod.EstEntries[race];
            _eqdpViewModel = eqdpViewModel;

            _eqdpViewModel.PropertyChanged += new PropertyChangedEventHandler(OnEqdpChanged);
            IsEnabled = _eqdpViewModel.Bit1;
        }

        private void OnEqdpChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(EqdpEntryViewModel.Bit1))
            {
                IsEnabled = _eqdpViewModel.Bit1;
            }
        }
    }
}
