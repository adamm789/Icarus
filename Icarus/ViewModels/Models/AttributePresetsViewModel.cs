using Icarus.Mods;
using Icarus.ViewModels.Mods;
using Icarus.ViewModels.Util;
using ItemDatabase;
using ItemDatabase.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.Models.DataContainers;

namespace Icarus.ViewModels.Models
{
    // TODO: Ability to create custom attribute presets?
    public class AttributePresetsViewModel : NotifyPropertyChanged
    {
        public AttributePresetsViewModel(string header, Dictionary<int, List<XivAttribute>> dict)
        {
            Header = header;
            Dictionary = dict;
            foreach (var key in dict.Keys)
            {
                var partHeader = "Part " + key;
                var part = new PartAttributesViewModel(partHeader, dict[key]);

                Presets.Add(part);
            }
        }

        public Dictionary<int, List<XivAttribute>> Dictionary;

        public string Header { get; set; }

        public ObservableCollection<PartAttributesViewModel> Presets { get; set; } = new();

        DelegateCommand? _copyPresetCommand;
        public DelegateCommand? CopyPresetCommand
        {
            get { return _copyPresetCommand; }
            set { _copyPresetCommand = value; OnPropertyChanged(); }
        }
    }
}
