using Icarus.Mods.DataContainers;
using Icarus.Services;
using Icarus.Services.Interfaces;
using Icarus.Services.UI;
using Icarus.ViewModels.Mods.DataContainers;
using Icarus.ViewModels.Mods.DataContainers.Interfaces;
using Icarus.ViewModels.Util;
using Icarus.Views.Import;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.ViewModels.Import
{
    public class ImportModPackViewModel : UIViewModelBase
    {
        readonly IWindowService _windowService;
        readonly ViewModelService _viewModelService;
        public ImportViewModel? ImportViewModel;

        public bool CanImportModPack => ModPacksAwaitingImport.Count > 0;
        public ImportModPackViewModel(ViewModelService viewModelService, IWindowService windowService, ILogService logService) : base(logService)
        {
            _windowService = windowService;
            _viewModelService = viewModelService;
        }
        public ObservableCollection<ModPackViewModel> ModPacksAwaitingImport { get; } = new();

        int _numMods = 0;
        public int NumMods
        {
            get { return _numMods; }
            set
            {
                _numMods = value;
                OnPropertyChanged();
                ImportAllCommand.RaiseCanExecuteChanged();
                RemoveSelectedCommand.RaiseCanExecuteChanged();
            }
        }

        int _numFiles = 0;
        public int NumFiles
        {
            get { return _numFiles; }
            set { _numFiles = value; OnPropertyChanged(); }
        }

        ImportSimpleTexToolsViewModel? _importSimpleTexTools;
        public ImportSimpleTexToolsViewModel? ImportSimpleTexTools
        {
            get { return _importSimpleTexTools; }
            set { _importSimpleTexTools = value; OnPropertyChanged(); }
        }

        DelegateCommand _importAllCommand;
        public DelegateCommand ImportAllCommand
        {
            get { return _importAllCommand ??= new DelegateCommand(_ => ImportAll(), _ => NumMods > 0); }
        }

        DelegateCommand _removeSelectedCommand;
        public DelegateCommand RemoveSelectedCommand
        {
            get { return _removeSelectedCommand ??= new DelegateCommand(_ => RemoveSelected(), _ => NumMods > 0); }
        }

        private void RemoveSelected()
        {
            // TODO: Prompt user confirmation for removal
            try
            {
                if (_toSimpleDict.ContainsKey(SelectedModPack))
                {
                    var import = _toSimpleDict[SelectedModPack];
                    import.ShouldRemove = true;
                }
            } catch (ArgumentNullException) {

                SelectedModPack = ModPacksAwaitingImport.FirstOrDefault();
            }
        }

        private void ImportAll()
        {
            var list = ModPacksAwaitingImport.ToList();
            foreach (var mp in list)
            {
                var simple = _toSimpleDict[mp];
                simple.ConfirmCommand();
            }
            SelectedModPack = null;
        }

        ModPackViewModel? _selectedModPack;
        public ModPackViewModel? SelectedModPack
        {
            get { return _selectedModPack; }
            set
            {
                _selectedModPack = value; OnPropertyChanged();
                if (value != null)
                {
                    if (_toSimpleDict.TryGetValue(value, out var _out))
                    {
                        ImportSimpleTexTools = _out;
                    }
                    else
                    {
                        ImportSimpleTexTools = null;
                    }
                }
                else
                {
                    ImportSimpleTexTools = null;
                }
            }
        }

        private void OnRemove(object sender, PropertyChangedEventArgs e)
        {
            if (sender is ImportSimpleTexToolsViewModel import && import.ShouldRemove == true)
            {
                var modPack = _toModPackDict[import];

                _toModPackDict.Remove(import);
                _toSimpleDict.Remove(modPack);

                ModPacksAwaitingImport.Remove(modPack);
                OnPropertyChanged(nameof(CanImportModPack));
                NumMods -= modPack.ModsListViewModel.SimpleModsList.Count;
                var eh = _ehDict[import];

                _ehDict.Remove(import);
                import.PropertyChanged -= eh;
                NumFiles--;

                // TODO: Upon removing a file, set SelectedModPack to ideally, the preceding entry
            }
        }

        public void Add(ModPack modPack)
        {
            var modPackViewModel = new ModPackViewModel(new ModPack(modPack), _viewModelService, _logService);
            modPackViewModel.Add(modPack);
            ModPacksAwaitingImport.Add(modPackViewModel);

            var _importSimple = new ImportSimpleTexToolsViewModel(modPackViewModel.ModsListViewModel, _logService);
            _importSimple.ImportViewModel = ImportViewModel;
            var eh = new PropertyChangedEventHandler(OnRemove);
            _importSimple.PropertyChanged += eh;

            _ehDict.Add(_importSimple, eh);
            _toModPackDict.Add(_importSimple, modPackViewModel);
            _toSimpleDict.Add(modPackViewModel, _importSimple);

            NumMods += modPackViewModel.ModsListViewModel.SimpleModsList.Count;
            NumFiles++;

            if (ModPacksAwaitingImport.Count == 1)
            {
                SelectedModPack = modPackViewModel;
            }

            OnPropertyChanged(nameof(CanImportModPack));

        }

        private Dictionary<ImportSimpleTexToolsViewModel, ModPackViewModel> _toModPackDict = new();
        private Dictionary<ModPackViewModel, ImportSimpleTexToolsViewModel> _toSimpleDict = new();
        private Dictionary<ImportSimpleTexToolsViewModel, PropertyChangedEventHandler> _ehDict = new();

        public void Show()
        {
            _windowService.Show<ImportModPackWindow>(this);
        }
    }
}
