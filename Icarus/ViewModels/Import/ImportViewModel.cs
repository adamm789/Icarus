using Icarus.Mods.DataContainers;
using Icarus.Mods.Interfaces;
using Icarus.Services.Files;
using Icarus.Services.Interfaces;
using Icarus.ViewModels.Mods.DataContainers.Interfaces;
using Icarus.ViewModels.Mods.DataContainers.ModPackList;
using Icarus.ViewModels.Util;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Icarus.ViewModels.Import
{
    public class ImportViewModel : ViewModelBase
    {
        readonly string _filter =
            "Valid Files | *.ttmp2; *.fbx; *.dds; *.png; *.bmp" +
            "|.ttmp2 | *.ttmp2" +
            "|.fbx | *.fbx" +
            "|.dds | *.dds" +
            "|.png | *.png" +
            "|.bmp | *.bmp";
        //var filter = "Valid Files | *.fbx; *.ttmp2; *.dds; *.png; *.bmp; *.mdl" + "|FBX File | *.fbx" + "|TexTools ModPack | *.ttmp2" + "| dds | *.dds" + "| png | *.png" + "| bmp | *.bmp";

        readonly IModPackViewModel _modPackViewModel;
        readonly ImportService _importService;
        readonly ISettingsService _settingsService;
        readonly ModPackListViewModel _modPackListViewModel;

        private string _initialDirectory = "";
        private OpenFileDialog _dlg;

        public ImportSimpleTexToolsViewModel ImportSimpleTexToolsViewModel;

        public ImportViewModel(IModPackViewModel modPack, ModPackListViewModel modPackList, ImportService importService,
            ISettingsService settingsService, ILogService logService, ImportSimpleTexToolsViewModel importSimpleTexToolsViewModel)
            : base(logService)
        {
            _settingsService = settingsService;
            _modPackViewModel = modPack;
            _modPackListViewModel = modPackList;
            _importService = importService;
            _logService = logService;
            ImportSimpleTexToolsViewModel = importSimpleTexToolsViewModel;

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

        // TODO: Ability to import only some of the mods in a ttmp2 file

        public async Task ImportFiles(IList<string> filePaths)
        {
            foreach (var path in filePaths)
            {
                var str = Path.Combine(path);
                if (File.Exists(str))
                {
                    _logService.Verbose($"Importing mod pack.");
                    //var modPack = await ImportFile(str);
                    var modPack = await Task.Run(async () => await ImportFile(str));
                    _logService.Verbose($"Finished importing.");

                    if (modPack.SimpleModsList.Count == 0)
                    {
                        _logService.Error($"Could not import {str}.");
                        return;
                    }

                    if (modPack.ModPackPages.Count > 0)
                    {
                        _modPackListViewModel.Add(modPack);
                    }
                    else
                    {
                        ImportSimpleTexToolsViewModel.Show();
                    }

                    _logService.Verbose($"Adding to mod list.");

                    // TODO: Display a "waiting window" on large modpacks?
                    if (modPack.SimpleModsList.Count > 0)
                    {

                    }
                    // TODO: Seems that this may freeze the UI on particularly large mod packs
                    _modPackViewModel.Add(modPack);
                    _logService.Verbose($"Finished adding.");
                }
            }
        }

        public async Task<ModPack> ImportFile(string filePath)
        {
            //return await Task.Run(() => _importService.ImportFile(filePath));
            var ext = Path.GetExtension(filePath);
            var retModPack = new ModPack();
            IMod? mod = null;
            if (ext == ".fbx")
            {
                mod = await _importService.TryImportModel(filePath);
            }
            else if (ext == ".dds")
            {
                mod = await Task.Run(() => _importService.TryImportDDS(filePath));
            }
            else if (ext == ".png" || ext == ".bmp")
            {
                mod = _importService.ImportTexture(filePath);
            }
            else if (ext == ".ttmp2")
            {
                if (_importService.IsSimpleModPack(filePath))
                {
                    // TODO: Window asking which mods the user wants to import
                }
                else
                {
                    retModPack = await _importService.TryImportTTModPack(filePath);
                }
            }
            if (retModPack == null)
            {
                return new ModPack();
            }
            if (mod != null)
            {
                retModPack.SimpleModsList.Add(mod);
            }
            return retModPack;
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

        private void SimpleTexToolsImportPrompt()
        {

        }
    }
}

