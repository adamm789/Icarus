using Icarus.Mods.DataContainers;
using Icarus.Mods.Interfaces;
using Icarus.Services;
using Icarus.Services.Files;
using Icarus.Services.Interfaces;
using Icarus.Util.Import;
using Icarus.ViewModels.Mods;
using Icarus.ViewModels.Mods.DataContainers;
using Icarus.ViewModels.Mods.DataContainers.Interfaces;
using Icarus.ViewModels.Mods.DataContainers.ModPackList;
using Icarus.ViewModels.Util;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Icarus.ViewModels.Import
{
    // TODO: Importing raw files goes in order of selection; if fbx is chosen first, this prevents other (possibly faster) files from importing until the fbx is done
    public class ImportViewModel : ViewModelBase
    {
        readonly string _filter =
            "Valid Files | *.ttmp2; *.fbx; *.dds; *.png; *.bmp" +
            "|.ttmp2 | *.ttmp2" +
            "|.fbx | *.fbx" +
            "|.dds | *.dds" +
            "|.png | *.png" +
            "|.bmp | *.bmp";

        readonly IModPackViewModel _modPackViewModel;
        readonly ImportService _importService;
        readonly ISettingsService _settingsService;
        readonly ModPackListViewModel _modPackListViewModel;

        private string _initialDirectory = "";

        public ImportModPackViewModel ImportModPackViewModel;

        public ImportViewModel(IModPackViewModel modPackViewModel, ViewModelService viewModelService, ImportService importService,
            ISettingsService settingsService, ILogService logService) : base(logService)
        {

        }

        public ImportViewModel(IModPackViewModel modPack, ModPackListViewModel modPackList, ImportService importService,
            ISettingsService settingsService, ImportModPackViewModel importSimpleTexToolsViewModel, ILogService logService)
            : base(logService)
        {
            _settingsService = settingsService;
            _modPackViewModel = modPack;
            _modPackListViewModel = modPackList;
            _importService = importService;
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
            get { return _onBrowseCommand ??= new DelegateCommand(async _ => await ImportFile()); }
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

        private async Task ImportFile()
        {
            if (String.IsNullOrWhiteSpace(_initialDirectory))
            {
                _initialDirectory = _settingsService.BrowseDirectory;
            }
            var dlg = new OpenFileDialog
            {
                Filter = _filter,
                InitialDirectory = _initialDirectory,
                Multiselect = true
            };
            dlg.ShowDialog();

            await ImportFiles(dlg.FileNames);
        }

        public async Task ImportFiles(IList<string> filePaths)
        {
            foreach (var path in filePaths)
            {
                var str = Path.Combine(path);
                if (File.Exists(str))
                {
                    _logService.Verbose($"Importing mod pack.");
                    //var modPack = await ImportFile(str);
                    var modPack = await ImportFile(str);
                    _logService.Verbose($"Finished importing.");

                    if (modPack.SimpleModsList.Count == 0)
                    {
                        _logService.Error($"Could not import {str}.");
                        continue;
                    }

                    if (modPack.ModPackPages.Count > 0)
                    {
                        _modPackListViewModel.Add(modPack);
                    }


                    if (modPack.SimpleModsList.Count == 1 && modPack.SimpleModsList[0].ImportSource == ImportSource.Raw)
                    {
                        _modPackViewModel.Add(modPack);
                    }
                    else
                    {
                        // TODO: How to handle partial imports of advanced mod packs?
                        // If not all mods are imported, should the mod pack pages be skipped?
                        ImportModPackViewModel.Add(modPack);
                    }
                }
            }
        }

        public async Task<ModPack> ImportFile(string filePath)
        {
            return await Task.Run(() => _importService.ImportFile(filePath));
        }

        public void Add(IEnumerable<ModViewModel> mods)
        {
            _modPackViewModel.AddRange(mods);
        }

        public bool CanAcceptFiles(StringCollection collection)
        {
            //var filter = "Valid Files | *.fbx; *.ttmp2; *.dds; *.png; *.bmp" + "|FBX File | *.fbx" + "|TexTools ModPack | *.ttmp2" + "| dds | *.dds" + "| png | *.png" + "| bmp | *.bmp";
            foreach (var str in collection)
            {
                var ext = Path.GetExtension(str);
                if (ext != ".fbx" && ext != ".ttmp2" && ext != ".dds" && ext != ".png" && ext != ".bmp")
                {
                    return false;
                }
            }
            return true;
        }
    }
}

