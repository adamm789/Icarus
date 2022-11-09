using Icarus.Services.Files;
using Icarus.Services.Interfaces;
using Icarus.ViewModels.Mods.DataContainers.Interfaces;
using Icarus.ViewModels.Util;
using Icarus.Views.Export;
using Serilog;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Icarus.ViewModels.Export
{
    public class ExportViewModel : NotifyPropertyChanged
    {
        readonly IMessageBoxService _messageBoxService;
        readonly ExportService _exportService;
        IWindowService _windowService;
        readonly IModsListViewModel _modPackViewModel;

        public ExportViewModel(IModsListViewModel modPack, IMessageBoxService messageBoxService, ExportService exportService, IWindowService windowService)
        {
            _modPackViewModel = modPack;
            _messageBoxService = messageBoxService;
            _exportService = exportService;
            _windowService = windowService;

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
        // TODO: Show user feedback that export is happening
        // Unsurprisingly, export raw does not really work with ivcs models (the model itself seems fine, but no armature and no weights)
        // It does seem to work with (at least) TexTools export
        // TODO: For raw exporter, allow options to include or not include pngs? or even certain files?
        public async Task Export(ExportType type)
        {
            IsBusy = true;
            ExportCommand.RaiseCanExecuteChanged();
            var path = _exportService.GetOutputPath(_modPackViewModel.ModPack, type);
            path = path.Replace('\\', '/');

            var shouldDelete = true;
            /*
            if (ExportType.Simple.HasFlag(type))
            {
                // TODO: Window that allows choosing which mods to export as well as a prompt
                var response = _windowService.ShowOptionWindow<SimpleExportSelectionWindow>(_modPackViewModel.SimpleModsList);
                if (response == DialogResult.No)
                {
                    shouldDelete = false;
                }
            }
            else
            {
            */
            // TODO: More accurate file/path existance and prompt
                if (File.Exists(path) || Directory.Exists(path))
                {
                    var message = $"The path {path} already exists. Overwrite?";
                    var response = _messageBoxService.ShowMessage(message, "File Exists", MessageBoxButtons.YesNo);
                    if (response == DialogResult.No)
                    {
                        shouldDelete = false;
                    }
                }
            //}
            string outputPath = "";
            var success = true;
            if (shouldDelete)
            {
                try
                {
                    outputPath = await _exportService.Export(_modPackViewModel.ModPack, type);
                    if (String.IsNullOrWhiteSpace(outputPath))
                    {
                        success = false;
                    }
                }
                catch (OperationCanceledException)
                {
                    success = false;
                    Log.Warning("Export was cancelled.");
                }
                catch (Exception ex)
                {
                    success = false;
                    Log.Error(ex, "Export threw an exception.");
                }
                if (success)
                {
                    DisplaySuccess(outputPath);
                }
                else
                {
                    DisplayFailure();
                }
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

        private void DisplaySuccess(string path = "")
        {
            _messageBoxService.Show($"Finished exporting {path}", "Finished Export", MessageBoxButtons.OK);
        }

        private void DisplayFailure()
        {
            //_messageBoxService.Show($"Failed to write ttmp2.", "", MessageBoxButtons.OK);
        }
    }
}
