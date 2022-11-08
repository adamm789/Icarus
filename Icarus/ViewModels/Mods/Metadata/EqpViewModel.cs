using Icarus.ViewModels.Util;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.Models.DataContainers;

namespace Icarus.ViewModels.Mods.Metadata
{
    public class EqpViewModel : NotifyPropertyChanged
    {
        public EqpViewModel(EquipmentParameter eqpEntry)
        {
            _eqpEntry = eqpEntry;
            UpdateEntries();
            SetPresets();
            UpdatePreset();
            //UpdateEntries();
            //SetPresets();
            //UpdatePreset();
        }

        EquipmentParameter _eqpEntry;

        ObservableCollection<EqpEntryViewModel> _availableFlags = new();
        public ObservableCollection<EqpEntryViewModel> AvailableFlags
        {
            get { return _availableFlags; }
            set { _availableFlags = value; OnPropertyChanged(); }
        }

        ObservableCollection<string> _presets = new();
        public ObservableCollection<string> Presets
        {
            get { return _presets; }
            set { _presets = value; OnPropertyChanged(); }
        }

        bool _simpleSelected = true;
        public bool SimpleSelected
        {
            get { return _simpleSelected; }
            set
            {
                _simpleSelected = value;
                OnPropertyChanged();
                UpdatePreset();
            }
        }

        int _index = -1;
        public int Index
        {
            get { return _index; }
            set
            {
                _index = value;
                OnPropertyChanged();
                if (value > 0 && value < Presets.Count)
                {
                    var str = Presets[_index];
                    _eqpEntry.SetBytes(_currDict[str]);
                    UpdateEntries();
                }
            }
        }

        private void UpdateEntries()
        {
            AvailableFlags.Clear();
            foreach (var kvp in _eqpEntry.GetFlags())
            {
                var flag = kvp.Key;
                var b = kvp.Value;

                var entry = new EqpEntryViewModel(flag, b);
                entry.PropertyChanged += new(OnEntryChanged);
                AvailableFlags.Add(entry);
            }
        }

        private void OnEntryChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(EqpEntryViewModel.EqpBool))
            {
                UpdatePreset();
            }
        }

        private void UpdatePreset()
        {
            var newFlags = AvailableFlags.ToDictionary(v => v.EqpFlag, v => v.EqpBool);
            _eqpEntry.SetFlags(newFlags);
            var bytes = _eqpEntry.GetBytes();

            for (var i = 0; i < _currDict.Count; i++)
            {
                var kvp = _currDict.ElementAt(i);
                if (kvp.Value.SequenceEqual(bytes))
                {
                    Index = i + 1;
                    return;
                }
            }
            Index = 0;
        }

        private void SetPresets()
        {
            var slot = _eqpEntry.Slot;
            if (_presetDict.ContainsKey(slot))
            {
                Presets = new(_presetDict[slot].Keys);
                Presets.Insert(0, "Custom");
                _currDict = _presetDict[slot];
            }
        }

        private Dictionary<string, byte[]> _currDict;

        // https://github.com/TexTools/FFXIV_TexTools_UI/blob/37290b2897c79dd1e913bb4ff90285f0e620ca9d/FFXIV_TexTools/Views/Metadata/EqpControl.xaml.cs#L179
        /// <summary>
        /// EQP Preset information.
        /// </summary>
        private static readonly Dictionary<string, Dictionary<string, byte[]>> _presetDict = new Dictionary<string, Dictionary<string, byte[]>>()
        {
            { "met", new Dictionary<string, byte[]>()
            {
                // 3 Bytes per
                { "Glasses", new byte [] { 225, 63, 3} },
                { "Hat", new byte [] { 227, 118, 3} },
                { "Open Helmet", new byte [] { 21, 240, 3} },
                { "Full Helmet", new byte [] { 23, 48, 3} },
            } },
            { "top", new Dictionary<string, byte[]>()
            {
                // 2 Bytes per3
                { "Sleeveless Top", new byte [] { 1, 63} },
                {  "Long-Sleeve Top", new byte [] { 115, 103 } },
                {  "Leotard", new byte [] { 1, 62 } },
                {  "Bodysuit", new byte [] { 1, 36 } },
            } },
            { "glv", new Dictionary<string, byte[]>()
            {
                // 1 Byte per
                {  "Bare Hands", new byte [] { 115 } },
                {  "Mid Gloves", new byte [] { 13 } },
                {  "Long Gloves", new byte [] { 15 } },
            } },
            { "dwn", new Dictionary<string, byte[]>()
            {
                // 1 Byte per
                {  "Shorts", new byte [] { 97 } },
                {  "Pants", new byte [] { 105 } },
                {  "Pants and Shoes", new byte [] { 65 } },
            } },
            { "sho", new Dictionary<string, byte[]>()
            {
                // 1 Byte per
                {  "Shoes", new byte [] { 3 } },
                {  "Mid Boots", new byte [] { 13 } },
                {  "Long Boots", new byte [] { 15 } },
            } }
        };
    }
}
