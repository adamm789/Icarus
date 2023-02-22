using Icarus.Mods.DataContainers;
using Icarus.Mods.Interfaces;
using Icarus.Services;
using Icarus.Services.Files;
using Icarus.Services.Interfaces;
using Icarus.Util.Import;
using Icarus.ViewModels.ModPackList;
using Icarus.ViewModels.Mods;
using Icarus.ViewModels.Mods.DataContainers;
using Icarus.ViewModels.Mods.DataContainers.Interfaces;
using Icarus.ViewModels.Util;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using xivModdingFramework.Materials.FileTypes;

namespace Icarus.ViewModels.Import
{
    // TODO: Importing raw files goes in order of selection; if fbx is chosen first, this prevents other (possibly faster) files from importing until the fbx is done
    public class ImportViewModel : ViewModelBase
    {
        readonly string _filter =
            "Valid Files | *.ttmp2; *.fbx; *.dds; *.png; *.bmp; *.pmp; *.mdl" +
            "|.ttmp2 | *.ttmp2" +
            "|.fbx | *.fbx" +
            "|.dds | *.dds" +
            "|.png | *.png" +
            "|.bmp | *.bmp" +
            "|.pmp | *.pmp" +
            "|.mdl | *.mdl";
        readonly List<string> _extensions = new List<string>()
        {
            ".ttmp2", ".fbx", ".dds", ".png", ".bmp", ".pmp", ".mdl"
        };

        readonly IModPackViewModel _modPackViewModel;
        readonly ImportService _importService;
        readonly ISettingsService _settingsService;
        readonly ModPackListViewModel _modPackListViewModel;
        readonly IMessageBoxService? _messageBoxService;

        private string _initialDirectory = "";

        public ImportModPackViewModel ImportModPackViewModel;

        public ImportViewModel(IModPackViewModel modPack, ModPackListViewModel modPackList, ImportService importService,
            ISettingsService settingsService, ImportModPackViewModel importSimpleTexToolsViewModel, IMessageBoxService? messageBoxService, ILogService? logService)
            : base(logService)
        {
            _settingsService = settingsService;
            _modPackViewModel = modPack;
            _modPackListViewModel = modPackList;
            _importService = importService;
            _messageBoxService = messageBoxService;
            _logService = logService;

            ImportModPackViewModel = importSimpleTexToolsViewModel;
            ImportModPackViewModel.ImportViewModel = this;

            var eh = new PropertyChangedEventHandler(OnPropertyChanged);
            _importService.PropertyChanged += eh;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_importService.IsImporting))
            {
                OnPropertyChanged(nameof(IsProgressIndeterminate));
            }
            if (e.PropertyName == nameof(_importService.ImportingFile))
            {
                OnPropertyChanged(nameof(ImportingFile));
            }
        }

        #region Bindings

        public string ImportingFile
        {
            get { return _importService.ImportingFile; }
        }

        DelegateCommand _onBrowseCommand;
        public DelegateCommand OnBrowseCommand
        {
            get { return _onBrowseCommand ??= new DelegateCommand(async _ => await ImportFiles()); }
        }

        DelegateCommand _onBrowseDirecoryCommand;
        public DelegateCommand OnBrowseDirecoryCommand
        {
            get { return _onBrowseDirecoryCommand ??= new DelegateCommand(async _ => await ImportDirectory()); }
        }

        double _individualProgressValue = 0;
        public double IndividualProgressValue
        {
            get { return _individualProgressValue; }
            set { _individualProgressValue = value; OnPropertyChanged(); }
        }

        double _totalProgressValue = 0;
        public double TotalProgressValue
        {
            get { return _totalProgressValue; }
            set { _totalProgressValue = value; OnPropertyChanged(); }
        }

        bool _isProgressIndeterminate = false;
        public bool IsProgressIndeterminate
        {
            get { return _importService.IsImporting; }
        }

        bool _overwritePages = true;
        public bool OverwritePages
        {
            get { return _overwritePages; }
            set { _overwritePages = value; OnPropertyChanged(); }
        }

        #endregion

        private async Task ImportFiles()
        {
            // TODO: Ability to import the raw penumbra directory (so I don't have to extract anything, which can be long, it seems)
            if (String.IsNullOrWhiteSpace(_initialDirectory))
            {
                _initialDirectory = _settingsService.BrowseDirectory;
            }

            using var dlg = new OpenFileDialog
            {
                Filter = _filter,
                InitialDirectory = _initialDirectory,
                Multiselect = true
            };

            dlg.ShowDialog();

            await ImportFiles(dlg.FileNames);
        }

        private async Task ImportDirectory()
        {
            if (String.IsNullOrWhiteSpace(_initialDirectory))
            {
                _initialDirectory = _settingsService.BrowseDirectory;
            }
            using var dlg = new FolderBrowserDialog
            {

            };
            dlg.ShowDialog();

            if (_importService.IsValidPenumbraDirectory(dlg.SelectedPath))
            {
                await ImportDirectory(dlg.SelectedPath);
            }
            else
            {
                var errorMessage = "Directory deemed invalid. \nMake sure the directory contains both \"default_mod.json\" and \"meta.json\"";
                _logService?.Error(errorMessage);
                _messageBoxService?.ShowMessage(errorMessage, "Invalid Penumbra Directory", MessageBoxButtons.OK);
            }
        }

        public async Task ImportDirectory(string dir)
        {
            _logService?.Verbose($"Importing penumbra directory: {dir}");
            var modPack = await Task.Run(() => _importService.ImportDirectory(dir));
            _logService?.Verbose($"Finished importing");

            var success = AddModPack(modPack);
            if (!success)
            {
                _logService?.Error($"Could not import Penumbra directory: {dir}");
            }
        }

        // TODO: Implement actual async import
        public async Task ImportFiles(IList<string> filePaths)
        {
            foreach (var path in filePaths)
            {
                var str = Path.Combine(path);
                if (File.Exists(str))
                {
                    _logService?.Verbose($"Importing mod pack.");
                    var modPack = await ImportFile(str);
                    _logService?.Verbose($"Finished importing.");

                    var success = AddModPack(modPack);
                    if (!success)
                    {
                        _logService?.Error($"Could not import {str}");
                    }
                }
            }
        }

        private bool AddModPack(ModPack modPack)
        {
            if (modPack.SimpleModsList.Count == 0)
            {
                return false;
            }

            if (modPack.ModPackPages.Count > 0)
            {
                _modPackListViewModel.Add(modPack);
            }

            if (modPack.SimpleModsList.Count == 1 && modPack.SimpleModsList[0].ImportSource == ImportSource.Raw || modPack.ModPackPages.Count > 0)
            {
                _modPackViewModel.Add(modPack);
            }
            else
            {
                // TODO: How to handle partial imports of advanced mod packs?
                // If not all mods are imported, should the mod pack pages be skipped?
                ImportModPackViewModel.Add(modPack);
            }
            return true;
        }

        public async Task<ModPack> ImportFile(string filePath)
        {
            return await Task.Run(() => _importService.ImportFile(filePath));
        }

        public void Add(IEnumerable<ModViewModel> mods)
        {
            _modPackViewModel.AddRange(mods);
        }
        public void Add(IModPackViewModel modPackViewModel)
        {
            var modsList = modPackViewModel.ModsListViewModel;
            var shouldImportPages = true;
            foreach (var mod in modsList.SimpleModsList)
            {
                if (!mod.ShouldImport)
                {
                    _logService?.Information("Not all mods were imported. Skipping mod pack pages.");
                    shouldImportPages = false;
                    break;
                }
            }
            if (shouldImportPages)
            {
                _logService?.Information($"Importing mod pack pages.");
                // TODO: modPackViewModel.ModPack here does not have the mod pack pages
                _modPackListViewModel.Add(modPackViewModel.ModPack);
            }
            _modPackViewModel.AddRange(modsList.SimpleModsList);
        }

        public bool CanAcceptFiles(StringCollection collection)
        {
            foreach (var str in collection)
            {
                var ext = Path.GetExtension(str);
                if (String.IsNullOrEmpty(ext))
                {
                    return false;
                }
                if (!_extensions.Contains(ext))
                {
                    return false;
                }
            }
            return true;
        }
    }
}

