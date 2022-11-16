using Icarus.Services.Interfaces;
using Icarus.ViewModels.Mods.DataContainers.Interfaces;
using Icarus.ViewModels.Mods.DataContainers.ModsList;
using Icarus.ViewModels.Util;
using Icarus.Views.Mods;
using ItemDatabase;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Forms;

namespace Icarus.ViewModels.Mods.DataContainers
{
    public class FilteredModsListViewModel : ViewModelBase
    {
        public ObservableCollection<ModViewModel> SimpleModsList { get; }

        Timer _timer = new();

        int numCalled = 0;
        readonly ILogService _logService;

        int _selectedIndex = 0;
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                _selectedIndex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedType));
            }
        }

        public Type SelectedType
        {
            get
            {
                switch (SelectedIndex)
                {
                    case 1:
                        return typeof(ModelModViewModel);
                    case 2:
                        return typeof(MaterialModViewModel);
                    case 3:
                        return typeof(TextureModViewModel);
                    case 4:
                        return typeof(MetadataModViewModel);
                    case 5:
                        return typeof(ReadOnlyModViewModel);
                    default:
                        return typeof(ModViewModel);
                }
            }
        }

        public FilteredModsListViewModel(IModsListViewModel modsListViewModel, ILogService logService) : base(logService)
        {
            SimpleModsList = modsListViewModel.SimpleModsList;
            _logService = logService;

            _timer.Tick += Timer_Tick;
            _timer.Interval = 300;

            AllMods = new(this, "All", logService);
            ModelMods = new(this, "Models", logService);
            MaterialMods = new(this, "Materials", logService);
            TextureMods = new(this, "Textures", logService);
            MetadataMods = new(this, "Metadata", logService);
            ReadOnlyMods = new(this, "ReadOnly", logService);
        }


        private void Timer_Tick(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(SearchTerm));
            _timer.Stop();
        }

        // TODO: Track some sort of "incompleteness"
        private void OnExportStatusChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is ModViewModel mod && e.PropertyName == nameof(ModViewModel.CanExport))
            {
                //IncompleteMods.Refresh();
            }
        }

        public bool SearchFilterFunction(object o)
        {
            if (o is ModViewModel mvm)
            {
                return mvm.HasMatch(SearchTerm);
            }
            else
            {
                return false;
            }
        }

        string _searchTerm = "";
        public string SearchTerm
        {
            get { return _searchTerm; }
            set
            {
                _searchTerm = value;

                // Delay calling Search() until user ostensibly stops typing
                _timer.Stop();
                _timer.Start();
            }
        }
        public FilteredTypeModsListViewModel<ModViewModel> AllMods { get; }
        public FilteredTypeModsListViewModel<ModelModViewModel> ModelMods { get; }
        public FilteredTypeModsListViewModel<MaterialModViewModel> MaterialMods { get; }
        public FilteredTypeModsListViewModel<TextureModViewModel> TextureMods { get; }
        public FilteredTypeModsListViewModel<MetadataModViewModel> MetadataMods { get; }
        public FilteredTypeModsListViewModel<ReadOnlyModViewModel> ReadOnlyMods { get; }
    }
}
