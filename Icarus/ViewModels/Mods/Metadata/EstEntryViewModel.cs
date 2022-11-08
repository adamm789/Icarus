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
using xivModdingFramework.Models.FileTypes;

namespace Icarus.ViewModels.Mods.Metadata
{
    public class EstEntryViewModel : NotifyPropertyChanged
    {
        // TODO: ComboBox of all available EstEntries...
        private static Dictionary<XivRace, HashSet<int>> _head;
        private static Dictionary<XivRace, HashSet<int>> _body;
        private static Dictionary<XivRace, HashSet<int>> _hair;
        private static Dictionary<XivRace, HashSet<int>> _face;

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
            if (mod.Slot == "met")
            {

                _head ??= Task.Run(() => Est.GetAllExtraSkeletons(Est.EstType.Head)).Result;
                AvailableSkeletonEntries = _head[race].ToList().ConvertAll(x => (ushort)x);
            }
            else if (mod.Slot == "top")
            {
                _body ??= Task.Run(() => Est.GetAllExtraSkeletons(Est.EstType.Body)).Result;
                AvailableSkeletonEntries = _body[race].ToList().ConvertAll(x => (ushort)x);
            }
            if (AvailableSkeletonEntries != null)
            {
                AvailableSkeletonEntries.Insert(0, 0);
            }

            _est = mod.EstEntries[race];
            _eqdpViewModel = eqdpViewModel;

            _eqdpViewModel.PropertyChanged += new PropertyChangedEventHandler(OnEqdpChanged);
            IsEnabled = _eqdpViewModel.Bit1;

            SkelId = _est.SkelId;
        }

        private void OnEqdpChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(EqdpEntryViewModel.Bit1))
            {
                IsEnabled = _eqdpViewModel.Bit1;
            }
        }

        public List<ushort>? AvailableSkeletonEntries { get; }
    }
}
