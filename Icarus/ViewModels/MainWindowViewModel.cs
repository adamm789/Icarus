using GongSolutions.Wpf.DragDrop;
using Icarus.Mods.DataContainers;
using Icarus.Services;
using Icarus.Services.Files;
using Icarus.Services.GameFiles;
using Icarus.Services.GameFiles.Interfaces;
using Icarus.Services.Interfaces;
using Icarus.ViewModels.Export;
using Icarus.ViewModels.Import;
using Icarus.ViewModels.Mods.DataContainers;
using Icarus.ViewModels.Mods.DataContainers.Interfaces;
using Icarus.ViewModels.Util;
using Icarus.Views;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using IDropTarget = GongSolutions.Wpf.DragDrop.IDropTarget;

[assembly: InternalsVisibleTo("UnitTests")]

namespace Icarus.ViewModels
{
    public class MainWindowViewModel : NotifyPropertyChanged, IDropTarget
    {
        private LuminaService _luminaService;
        private IWindowService _windowService = ServiceManager.GetRequiredService<IWindowService>();
        private AppSettingsViewModel _appSettings;
        private UserPreferencesViewModel _userPreferences;

        public MainWindowViewModel()
        {
            Init();
        }

        private void Init()
        {
            var modPack = new ModPack();
            _luminaService = ServiceManager.GetRequiredService<LuminaService>();
            var _userPreferencesService = ServiceManager.GetRequiredService<IUserPreferencesService>();
            _userPreferences = new(_userPreferencesService);
            var x = 0;

            var _messageBoxService = ServiceManager.GetRequiredService<IMessageBoxService>();
            var _exportService = ServiceManager.GetRequiredService<ExportService>();
            var _itemDatabaseService = ServiceManager.GetRequiredService<ItemListService>();
            var _importService = ServiceManager.GetRequiredService<ImportService>();
            var _modFileService = ServiceManager.GetRequiredService<ViewModelService>();
            var settingsService = ServiceManager.GetRequiredService<SettingsService>();
            var gameFileDataService = ServiceManager.GetRequiredService<IGameFileService>();
            var logService = ServiceManager.GetRequiredService<ILogService>();

            _appSettings = new(settingsService, _messageBoxService);
            _luminaService.TrySetLumina();

            ModPackMetaViewModel = new ModPackMetaViewModel(modPack, _userPreferencesService);
            ModsListViewModel = new ModPackModsListViewModel(modPack, _modFileService);
            ModPackViewModel = new ModPackViewModel(modPack, ModPackMetaViewModel, ModsListViewModel, _modFileService);
            SearchViewModel = new(_itemDatabaseService, logService);

            ImportVanillaViewModel = new(ModsListViewModel, SearchViewModel, gameFileDataService, logService);
            ExportViewModel = new(ModsListViewModel, _messageBoxService, _exportService);
            ImportViewModel = new(ModPackViewModel, _importService, settingsService, logService);


            var exportStatusChange = new PropertyChangedEventHandler(OnExportStatusChanged);
            ExportViewModel.PropertyChanged += exportStatusChange;

            var settingsChangedEventHandler = new PropertyChangedEventHandler(SettingChanged);
            _luminaService.PropertyChanged += settingsChangedEventHandler;

            var isImportingAdvanced = new PropertyChangedEventHandler(OnAdvancedImport);
            _importService.PropertyChanged += isImportingAdvanced;
        }

        private void OnExportStatusChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ExportViewModel.IsBusy))
            {
                OnPropertyChanged(nameof(CanExport));
            }
        }

        private void OnAdvancedImport(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ImportService.IsImportingAdvanced))
            {
                var import = sender as ImportService;
                CanOpenAdvanced = !import.IsImportingAdvanced;
            }
        }

        bool _canOpenAdvanced = true;
        public bool CanOpenAdvanced
        {
            get { return _canOpenAdvanced; }
            set { _canOpenAdvanced = value; OnPropertyChanged(); }
        }

        public bool CanExport
        {
            get { return !ExportViewModel.IsBusy; }
        }

        #region Bindings

        IModsListViewModel _modsListViewModel;
        public IModsListViewModel ModsListViewModel
        {
            get { return _modsListViewModel; }
            set { _modsListViewModel = value; OnPropertyChanged(); }
        }

        IModPackViewModel _modPackViewModel;
        public IModPackViewModel ModPackViewModel
        {
            get { return _modPackViewModel; }
            set { _modPackViewModel = value; OnPropertyChanged(); }
        }
        IModPackMetaViewModel _modPackMetaViewModel;
        public IModPackMetaViewModel ModPackMetaViewModel
        {
            get { return _modPackMetaViewModel; }
            set { _modPackMetaViewModel = value; OnPropertyChanged(); }
        }
        ExportViewModel _exportViewModel;
        public ExportViewModel ExportViewModel
        {
            get { return _exportViewModel; }
            set { _exportViewModel = value; OnPropertyChanged(); }
        }

        ImportViewModel _importViewModel;
        public ImportViewModel ImportViewModel
        {
            get { return _importViewModel; }
            set { _importViewModel = value; OnPropertyChanged(); }
        }

        SearchViewModel _searchViewModel;
        public SearchViewModel SearchViewModel
        {
            get { return _searchViewModel; }
            set { _searchViewModel = value; OnPropertyChanged(); }
        }

        bool _isBusy = false;
        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                OnPropertyChanged();
                OpenSettings.RaiseCanExecuteChanged();
            }
        }

        public bool GamePathSet
        {
            get { return _luminaService.IsLuminaSet; }
        }

        DelegateCommand _openSettings;
        public DelegateCommand OpenSettings
        {
            get { return _openSettings ??= new DelegateCommand(o => OpenSettingsWindow(), o => !IsBusy); }
        }

        DelegateCommand _openPreferences;
        public DelegateCommand OpenPreferences
        {
            get { return _openPreferences ??= new DelegateCommand(o => OpenUserPreferencesWindow(), o => !IsBusy); }
        }
        ImportVanillaViewModel _importVanillaViewModel;
        public ImportVanillaViewModel ImportVanillaViewModel
        {
            get { return _importVanillaViewModel; }
            set { _importVanillaViewModel = value; }
        }
        #endregion

        private void SettingChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(GamePathSet));
        }

        public void OpenSettingsWindow()
        {
            _windowService.ShowWindow<AppSettingsWindow>(_appSettings);
        }

        public void OpenUserPreferencesWindow()
        {
            _windowService.ShowWindow<UserPreferencesWindow>(_userPreferences);
        }

        public void DragOver(IDropInfo dropInfo)
        {
            var dataObject = dropInfo.Data as System.Windows.DataObject;
            var target = dropInfo.TargetItem;
            if (dataObject != null && dataObject.GetDataPresent(DataFormats.FileDrop))
            {
                var s = dataObject.GetFileDropList();
                if (!ExportViewModel.IsBusy && ImportViewModel.CanAcceptFiles(s))
                {
                    dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                    dropInfo.Effects = System.Windows.DragDropEffects.Copy;
                }
            }
            else
            {
                dropInfo.NotHandled = true;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            var dataObject = dropInfo.Data as DataObject;
            if (dataObject != null && dataObject.ContainsFileDropList())
            {
                ImportViewModel.ImportFiles(dataObject.GetFileDropList().Cast<string>().ToList());
            }
            else
            {
                dropInfo.NotHandled = true;
            }
        }
    }
}
