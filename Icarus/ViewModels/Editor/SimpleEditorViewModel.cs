using Icarus.Services;
using Icarus.Services.Interfaces;
using Icarus.ViewModels.Export;
using Icarus.ViewModels.Import;
using Icarus.ViewModels.Items;
using Icarus.ViewModels.Mods.DataContainers;
using Icarus.ViewModels.Mods.DataContainers.Interfaces;
using Icarus.ViewModels.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.ViewModels.Editor
{
    // TODO: Very short freeze when ctrl+c
    public class SimpleEditorViewModel : NotifyPropertyChanged
    {
        // TODO: Create a "trash can" for "deleted" mods
        public ImportViewModel ImportViewModel { get; }
        public IModPackMetaViewModel ModPackMetaViewModel { get; }
        public IModsListViewModel ModsListViewModel { get; }
        public ExportViewModel ExportViewModel { get; }
        public ImportModPackViewModel ImportModPackViewModel { get; }

        // TODO: Extract these into its own class (?)
        // Will also need to create a View for it
        public ItemListViewModel ItemListViewModel { get; }
        public ImportVanillaViewModel ImportVanillaViewModel { get; }

        public SimpleEditorViewModel(IModPackViewModel modPack, ItemListViewModel itemList,
            ImportViewModel import, ExportViewModel export, ImportVanillaViewModel importVanillaViewModel, ImportModPackViewModel importModPackViewModel)
        {
            ModPackMetaViewModel = modPack.ModPackMetaViewModel;
            ModsListViewModel = modPack.ModsListViewModel;

            ItemListViewModel = itemList;
            ImportViewModel = import;
            ExportViewModel = export;

            ImportVanillaViewModel = importVanillaViewModel;
            ImportModPackViewModel = importModPackViewModel;

            ImportModPackViewModel.PropertyChanged += new(OnNumModsChanged);
            UpdateImportAllText();
        }

        private void OnNumModsChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is ImportModPackViewModel && (e.PropertyName == nameof(ImportModPackViewModel.NumMods) || e.PropertyName == nameof(ImportModPackViewModel.NumFiles)))
            {
                _numMods = ImportModPackViewModel.NumMods;
                UpdateImportAllText();
                OpenImportWindowCommand.RaiseCanExecuteChanged();
            }
        }

        int _numMods;

        private void UpdateImportAllText()
        {
            ImportAllText = $"Import all {ImportModPackViewModel.NumMods} mod(s)";
            ImportCommandText = $"Inspect {ImportModPackViewModel.NumFiles} modpack(s)";
        }

        DelegateCommand _openImportWindowCommand;
        public DelegateCommand OpenImportWindowCommand
        {
            get { return _openImportWindowCommand ??= new DelegateCommand(_ => OpenSimpleTexToolsImportWindow(), _ => _numMods > 0); }
        }

        public DelegateCommand ImportAllCommand => ImportModPackViewModel.ImportAllCommand;

        string _importAllText = "";
        public string ImportAllText
        {
            get { return _importAllText; }
            set { _importAllText = value; OnPropertyChanged(); }
        }

        string _importCommandText = "";
        public string ImportCommandText
        {
            get { return _importCommandText; }
            set { _importCommandText = value; OnPropertyChanged(); }
        }

        private void OpenSimpleTexToolsImportWindow()
        {
            ImportModPackViewModel.Show();
        }
    }
}
