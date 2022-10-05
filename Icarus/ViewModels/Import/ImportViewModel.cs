using Icarus.Mods;
using Icarus.Mods.DataContainers;
using Icarus.Services;
using Icarus.Services.Files;
using Icarus.Services.GameFiles;
using Icarus.Services.Interfaces;
using Icarus.Util;
using Icarus.ViewModels.Mods.DataContainers;
using Icarus.ViewModels.Mods.DataContainers.Interfaces;
using Icarus.ViewModels.Util;
using ItemDatabase.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using xivModdingFramework.General.Enums;
using xivModdingFramework.Models.DataContainers;

namespace Icarus.ViewModels.Import
{
    public class ImportViewModel : NotifyPropertyChanged
    {
        public static string FilePathLabel = "File path";
        public static string ReplacementPathLabel = "In-Game";
        public static string OpenEditorLabel = "Open Editor";

        readonly IModPackViewModel _modPackViewModel;
        readonly ImportService _importService;
        readonly ILogService _logService;
        readonly ISettingsService _settingsService;

        public ImportViewModel(IModPackViewModel modPack, ImportService importService, ISettingsService settingsService, ILogService logService)
        {
            _settingsService = settingsService;
            _modPackViewModel = modPack;
            _importService = importService;
            _logService = logService;

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
            get { return _onBrowseCommand ??= new DelegateCommand(o => ImportFile()); }
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

        // Import File -> Convert to TTModel with converter.exe (TTModel Imported)
        // Choose destination item -> XivMdl OgMdl -> TTModel Original
        // WriteModelToBytes (Imported, OgMdl)
        // new Mdl(Imported, OgMdl).MakeNewMdlFile
        private async Task ImportFile()
        {
            var filter = "Valid Files | *.fbx; *.ttmp2; *.dds; *.png" + "|FBX File | *.fbx" + "|TexTools ModPack | *.ttmp2" + "| dds | *.dds" + "| png | *.png";

            //var filter = "Valid Files | *.fbx; *.ttmp2; *.dds" + "|FBX File | *.fbx" + "|TexTools ModPack | *.ttmp2" + "| dds | *.dds";
            //var filter = "Valid Files | *.fbx; *.ttmp2" + "|FBX File | *.fbx" + "|TexTools ModPack | *.ttmp2";

            var dlg = new OpenFileDialog
            {
                // TODO: Default folder
                Filter = filter,
                InitialDirectory = _settingsService.BrowseDirectory,
                Multiselect = true
            };
            dlg.ShowDialog();

            // TODO?: Import asynchronously?
            foreach (var file in dlg.FileNames)
            {
                var str = Path.Combine(file);
                // TODO: Consider what happens when importing multiple files
                // Particularly, e.g. a ttmp2 and an fbx file
                if (File.Exists(str))
                {
                    var modPack = await ImportFile(str);

                    if (modPack == null)
                    {
                        _logService.Error("Could not get mod pack.");
                        return;
                    }

                    // TODO: Seems that this may freeze the UI on particularly large mod packs
                    // TODO: Ask for user confirmation

                    // Three options:
                    // Import only mods
                    // Import and overwrite ModPackPages
                    // Cancel

                    // If add only mods
                    // ModPack.AddMods(mods);

                    // else if add mods and overwrite modpack
                    ModPackViewModelImportFlags flags = ModPackViewModelImportFlags.AddMods;
                    if (modPack.ModPackPages.Count > 0)
                    {
                        // TODO: Window to show import options
                        flags |= ModPackViewModelImportFlags.AppendPagesToEnd;
                        flags |= ModPackViewModelImportFlags.OverwriteData;
                    }
                    _logService.Verbose($"Setting modpack. {modPack.SimpleModsList.Count} simple mods and {modPack.ModPackPages.Count} pack pages");
                    _modPackViewModel.SetModPack(modPack, flags);
                }
            }
        }

        public async Task<ModPack?> ImportFile(string filePath)
        {
            return await _importService.ImportFile(filePath);
        }
        // TODO: Create an "empty" MaterialMod?
    }
}

