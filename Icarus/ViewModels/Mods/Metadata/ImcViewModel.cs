using Icarus.ViewModels.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.Variants.DataContainers;
using static HelixToolkit.SharpDX.Core.Model.Metadata;

namespace Icarus.ViewModels.Mods.Metadata
{
    public class ImcViewModel : NotifyPropertyChanged
    {
        public ObservableCollection<ImcEntryViewModel> Entries { get; } = new();
        public ObservableCollection<int> AvailableEntries { get; } = new();

        public ImcViewModel(List<XivImc> entries)
        {
            for (var i = 0; i < entries.Count; i++)
            {
                var imc = new ImcEntryViewModel(entries[i]);
                Entries.Add(imc);
                AvailableEntries.Add(i);
            }
            SelectedIndex = 0;
        }

        DelegateCommand _addEntryCommand;
        public DelegateCommand AddEntryCommand
        {
            get { return _addEntryCommand ??= new DelegateCommand(_ => AddEntry()); }
        }

        private void AddEntry()
        {
            var imc = new XivImc();
            var vm = new ImcEntryViewModel(imc);
            Entries.Add(vm);
            AvailableEntries.Add(AvailableEntries.Count + 1);
        }

        int _selectedIndex;
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set {
                _selectedIndex = value;
                OnPropertyChanged();
                DisplayedEntry = Entries[value];
            }
        }

        ImcEntryViewModel _displayedEntry;
        public ImcEntryViewModel DisplayedEntry
        {
            get { return _displayedEntry; }
            set { _displayedEntry = value; OnPropertyChanged(); }
        }
    }
}
