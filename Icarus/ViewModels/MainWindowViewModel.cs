using GongSolutions.Wpf.DragDrop;
using Icarus.Mods.DataContainers;
using Icarus.Services;
using Icarus.Services.Files;
using Icarus.Services.GameFiles;
using Icarus.Services.GameFiles.Interfaces;
using Icarus.Services.Interfaces;
using Icarus.ViewModels.Editor;
using Icarus.ViewModels.Export;
using Icarus.ViewModels.Import;
using Icarus.ViewModels.Items;
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

        public MainWindowViewModel()
        {
            var modPack = new ModPack();
            _luminaService = ServiceManager.GetRequiredService<LuminaService>();
            UserPreferencesService = ServiceManager.GetRequiredService<IUserPreferencesService>();

            // TOOD: Clean up the mess in MainWindowViewModel...?
            var _messageBoxService = ServiceManager.GetRequiredService<IMessageBoxService>();
            var _exportService = ServiceManager.GetRequiredService<ExportService>();
            var itemListService = ServiceManager.GetRequiredService<IItemListService>();
            var _importService = ServiceManager.GetRequiredService<ImportService>();
            var viewModelService = ServiceManager.GetRequiredService<ViewModelService>();
            var settingsService = ServiceManager.GetRequiredService<ISettingsService>();
            var gameFileDataService = ServiceManager.GetRequiredService<IGameFileService>();
            var logService = ServiceManager.GetRequiredService<ILogService>();

            _logViewModel = new LogViewModel(logService);
            _appSettings = new(settingsService, _messageBoxService);
            _luminaService.TrySetLumina();

            ModPackViewModel = new ModPackViewModel(modPack, viewModelService, logService);
            //ModPackMetaViewModel = ModPackViewModel.ModPackMetaViewModel;
            //ModsListViewModel = ModPackViewModel.ModsListViewModel;
            //FilteredModsListViewModel = ModsListViewModel.FilteredModsList;

            ModPackListViewModel = new ModPackListViewModel(ModPackViewModel.ModsListViewModel, viewModelService, logService);

            ItemListViewModel = new(itemListService, logService);
            ImportVanillaViewModel = new(ModPackViewModel.ModsListViewModel, ItemListViewModel, gameFileDataService, logService);
            ExportViewModel = new(ModPackViewModel.ModsListViewModel, _messageBoxService, _exportService);
            ImportViewModel = new(ModPackViewModel, ModPackListViewModel, _importService, settingsService, logService);

            SimpleEditorViewModel = new(ModPackViewModel, ItemListViewModel, ImportVanillaViewModel, ImportViewModel, ExportViewModel);
            AdvancedEditorViewModel = new(ModPackViewModel, ExportViewModel, ModPackListViewModel);

            var exportStatusChange = new PropertyChangedEventHandler(OnExportStatusChanged);
            ExportViewModel.PropertyChanged += exportStatusChange;

            var settingsChangedEventHandler = new PropertyChangedEventHandler(SettingChanged);
            _luminaService.PropertyChanged += settingsChangedEventHandler;

            var isImportingAdvanced = new PropertyChangedEventHandler(OnAdvancedImport);
            _importService.PropertyChanged += isImportingAdvanced;
        }

        LogViewModel _logViewModel;

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
        public SimpleEditorViewModel SimpleEditorViewModel { get; }
        public AdvancedEditorViewModel AdvancedEditorViewModel { get; }

        public IModsListViewModel ModsListViewModel => ModPackViewModel.ModsListViewModel;
        public IModPackViewModel ModPackViewModel { get; set; }
        public IModPackMetaViewModel ModPackMetaViewModel => ModPackViewModel.ModPackMetaViewModel;
        public ExportViewModel ExportViewModel { get; set; }
        public ImportViewModel ImportViewModel { get; set; }
        public ImportVanillaViewModel ImportVanillaViewModel { get; set; }
        public ItemListViewModel ItemListViewModel { get; set; }
        public FilteredModsListViewModel FilteredModsListViewModel { get; set; }
        public IUserPreferencesService UserPreferencesService { get; set; }
        public ModPackListViewModel ModPackListViewModel { get; set; }

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
            get { return _openSettings ??= new DelegateCommand(_ => OpenSettingsWindow(), _ => !IsBusy); }
        }

        DelegateCommand _openPreferences;
        public DelegateCommand OpenPreferences
        {
            get { return _openPreferences ??= new DelegateCommand(_ => OpenUserPreferencesWindow(), _ => !IsBusy); }
        }

        DelegateCommand _openLog;
        public DelegateCommand OpenLog
        {
            get { return _openLog ??= new DelegateCommand(_ => OpenLogWindow()); }
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
            _windowService.ShowWindow<UserPreferencesWindow>(UserPreferencesService);
        }

        public void OpenLogWindow()
        {
            if (!_windowService.IsWindowOpen<LogWindow>())
            {
                _windowService.Show<LogWindow>(_logViewModel);
            }
        }
        void IDropTarget.DragOver(IDropInfo dropInfo)
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

        void IDropTarget.Drop(IDropInfo dropInfo)
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
