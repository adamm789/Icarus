using Icarus.ViewModels.Util;
using ItemDatabase.Enums;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Icarus.ViewModels.Models
{
    // TODO: Ability to create custom attribute presets?
    public class AttributePresetsViewModel : NotifyPropertyChanged
    {
        public AttributePresetsViewModel(string header, Dictionary<int, List<string>> dict)
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

        public Dictionary<int, List<string>> Dictionary;

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
