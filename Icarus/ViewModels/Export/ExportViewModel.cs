using Icarus.Mods.DataContainers;
using Icarus.Services.Files;
using Icarus.Services.Interfaces;
using Icarus.Util;
using Icarus.ViewModels.Import;
using Icarus.ViewModels.Mods;
using Icarus.ViewModels.Mods.DataContainers.Interfaces;
using Icarus.ViewModels.Util;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Icarus.ViewModels.Export
{
    public class ExportViewModel : NotifyPropertyChanged
    {
        readonly IMessageBoxService _messageBoxService;
        readonly ExportService _exportService;

        readonly IModsListViewModel _modPackViewModel;

        public ExportViewModel(IModsListViewModel modPack, IMessageBoxService messageBoxService, ExportService exportService)
        {
            _modPackViewModel = modPack;
            _messageBoxService = messageBoxService;
            _exportService = exportService;

            var eh = new PropertyChangedEventHandler(OnPropertyChanged);
            _modPackViewModel.PropertyChanged += eh;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_modPackViewModel.CanExport))
            {
                OnPropertyChanged(nameof(CanExport));
                ExportCommand.RaiseCanExecuteChanged();
            }
        }

        // TODO: Somehow show progress
        // TODO: Implement cancellation
        public async Task Export(ExportType type)
        {
            IsBusy = true;
            ExportCommand.RaiseCanExecuteChanged();
            var path = _exportService.GetOutputPath(_modPackViewModel.ModPack, type);
            path = path.Replace('\\', '/');

            var shouldDelete = true;
            if (File.Exists(path) || Directory.Exists(path))
            {
                var message = String.Format("The path {0} already exists. Overwrite?", path);
                var response = _messageBoxService.ShowMessage(message, "File Exists", MessageBoxButtons.YesNo);
                if (response == DialogResult.No)
                {
                    shouldDelete = false;
                }
            }
            string outputPath = "";
            // TODO: Show user (better) feedback that export is happening
            var success = true;
            if (shouldDelete)
            {
                try
                {
                    outputPath = await _exportService.Export(_modPackViewModel.ModPack, type);
                }
                catch (OperationCanceledException)
                {
                    success = false;
                    Log.Warning("Export was cancelled.");
                }
            }
            if (success)
            {
                DisplayFinished(outputPath);
            }
            IsBusy = false;
            ExportCommand.RaiseCanExecuteChanged();
        }

        DelegateCommand _exportCommand;
        public DelegateCommand ExportCommand
        {
            get { return _exportCommand ??= new DelegateCommand(async o => await Export((ExportType)o), o => CanExport); }
        }

        bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public bool CanExport => _modPackViewModel.CanExport && !IsBusy;

        private void DisplayFinished(string path = "")
        {
            path = path.Replace('\\', '/');
            _messageBoxService.Show("Finished exporting " + path, "Finished Export", MessageBoxButtons.OK);
        }
    }
}
