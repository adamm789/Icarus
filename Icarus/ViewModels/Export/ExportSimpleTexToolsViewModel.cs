using Icarus.Services.Interfaces;
using Icarus.ViewModels.Mods;
using Icarus.ViewModels.Mods.DataContainers;
using Icarus.ViewModels.Mods.DataContainers.Interfaces;
using Icarus.ViewModels.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.ViewModels.Export
{
    // TODO: Prompt for user to delete any files if it exists, upon pressing "Confirm"
    public class ExportSimpleTexToolsViewModel : UIViewModelBase
    {
        IModsListViewModel _modsListViewModel;

        FilteredModsListViewModel _filteredMods;
        public FilteredModsListViewModel FilteredMods { get; set; }
        public bool? DialogResult { get; set; }

        public bool ShouldDelete { get; set; } = false;

        bool _shouldExportAll = true;
        public bool ShouldExportAll
        {
            get { return _shouldExportAll; }
            set
            {
                _shouldExportAll = value;
                OnPropertyChanged();
                foreach (var m in _modsListViewModel.SimpleModsList)
                {
                    if (_selectedType.IsInstanceOfType(m))
                    {
                        m.ShouldExport = value;
                    }
                }
            }
        }

        string _shouldExportAllText;
        public string ShouldExportAllText
        {
            get { return _shouldExportAllText; }
            set { _shouldExportAllText = value; OnPropertyChanged(); }
        }

        string _confirmText = "";
        public string ConfirmText
        {
            get { return _confirmText; }
            set { _confirmText = value; OnPropertyChanged(); }
        }

        public ExportSimpleTexToolsViewModel(IModsListViewModel modsListViewModel, ILogService logService) : base(logService)
        {
            _modsListViewModel = modsListViewModel;
            FilteredMods = new(_modsListViewModel, logService);
            _eh = new(OnModsListPropertyChanged);

            FilteredMods.PropertyChanged += new(OnFilteredModsListPropertyChanged);
            _modsListViewModel.SimpleModsList.CollectionChanged += new(OnCollectionChanged);

            UpdateText();
        }

        private void OnModsListPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ModViewModel.ShouldExport) && sender is ModViewModel mvm)
            {
                UpdateText();
            }
        }

        private void OnFilteredModsListPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is FilteredModsListViewModel modsList && e.PropertyName == nameof(FilteredModsListViewModel.SelectedType))
            {
                _selectedType = modsList.SelectedType;
                if (modsList.SelectedType == typeof(ModelModViewModel)) {
                    _modType = "Models";
                }
                else if(modsList.SelectedType == typeof(MaterialModViewModel))
                {
                    _modType = "Materials";
                }
                else if(modsList.SelectedType == typeof(TextureModViewModel))
                {
                    _modType = "Textures";
                }
                else if(modsList.SelectedType == typeof(MetadataModViewModel))
                {
                    _modType = "Metadata";
                }
                else if(modsList.SelectedType == typeof(ReadOnlyModViewModel)) {
                    _modType = "Metadata";
                }
                else
                {
                    _modType = "";
                }
                UpdateText();
            }
        }

        private Type _selectedType = typeof(ModViewModel);

        PropertyChangedEventHandler _eh;

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                var mods = e.NewItems.Cast<ModViewModel>();
                foreach (var m in mods)
                {
                    m.PropertyChanged += _eh;
                }
                UpdateText();
            }
            if (e.OldItems != null)
            {
                UpdateText();
            }
        }

        string _modType = "";

        private void UpdateText()
        {
            var selectedTypeList = _modsListViewModel.SimpleModsList.Where(m => _selectedType.IsInstanceOfType(m));
            var numSelected = selectedTypeList.Where(m => m.ShouldExport).Count();

            ConfirmText = $"Export {numSelected}/{selectedTypeList.Count()} mods";

            if (numSelected != selectedTypeList.Count())
            {
                _shouldExportAll = false;
                ShouldExportAllText = $"Select All {_modType}";
            }
            else
            {
                _shouldExportAll = true;
                ShouldExportAllText = $"Unselect All {_modType}";
            }
            OnPropertyChanged(nameof(ShouldExportAll));
        }

        DelegateCommand _onConfirmCommand;
        public DelegateCommand OnConfirmCommand
        {
            get { return _onConfirmCommand ??= new DelegateCommand(_ => { ShouldDelete = true; CloseAction?.Invoke(); }); }
        }

        DelegateCommand _onCancelCommand;
        public DelegateCommand OnCancelCommand
        {
            get { return _onCancelCommand ??= new DelegateCommand(_ => { ShouldDelete = false; CloseAction?.Invoke(); }); }
        }

        DelegateCommand _invertSelectionCommand;
        public DelegateCommand InvertSelectionCommand
        {
            get { return _invertSelectionCommand ??= new DelegateCommand(_ => InvertSelection()); }
        }

        private void InvertSelection()
        {
            foreach (var m in _modsListViewModel.SimpleModsList)
            {
                if (_selectedType.IsInstanceOfType(m))
                {
                    m.ShouldExport = !m.ShouldExport;
                }
            }
        }
    }
}
