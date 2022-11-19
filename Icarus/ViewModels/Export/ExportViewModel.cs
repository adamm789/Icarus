using Icarus.Services.Files;
using Icarus.Services.Interfaces;
using Icarus.ViewModels.Mods.DataContainers.Interfaces;
using Icarus.ViewModels.Util;
using Icarus.Views.Export;
using Serilog;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Icarus.ViewModels.Export
{
    public class ExportViewModel : ViewModelBase
    {
        readonly IMessageBoxService _messageBoxService;
        readonly ExportService _exportService;
        readonly IWindowService _windowService;
        readonly IModsListViewModel _modsListViewModel;
        readonly ILogService _logService;

        ExportSimpleTexToolsViewModel ExportSimpleTexToolsViewModel;

        public ExportViewModel(IModsListViewModel modsListViewModel, IMessageBoxService messageBoxService, ExportService exportService, IWindowService windowService, ILogService logService)
            : base(logService)
        {
            _modsListViewModel = modsListViewModel;
            _messageBoxService = messageBoxService;
            _exportService = exportService;
            _windowService = windowService;
            _logService = logService;

            ExportSimpleTexToolsViewModel = new(modsListViewModel, _logService);

            var eh = new PropertyChangedEventHandler(OnPropertyChanged);
            _modsListViewModel.PropertyChanged += eh;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_modsListViewModel.CanExport))
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
            try
            {
                var path = _exportService.GetOutputPath(_modsListViewModel.ModPack, type);
                path = path.Replace('\\', '/');

                var shouldDelete = true;

                // TODO: Allow key combination to bypass this and always do the opposite
                // Will also need to export all if asking is overridden
                if (ExportType.Simple.HasFlag(type))
                {
                    var result = _windowService.ShowWindow<ExportSimpleTexToolsWindow>(ExportSimpleTexToolsViewModel);
                    shouldDelete = ExportSimpleTexToolsViewModel.ShouldDelete;
                    ExportSimpleTexToolsViewModel.ShouldDelete = false;
                }
                else
                {
                    // TODO: More accurate file/path existance and prompt
                    if (File.Exists(path) || Directory.Exists(path))
                    {
                        var message = $"The path {path} already exists. Overwrite?";
                        var result = _messageBoxService.ShowMessage(message, "File Exists", MessageBoxButtons.YesNo);
                        if (result == DialogResult.No)
                        {
                            shouldDelete = false;
                        }
                    }
                }
                string outputPath = "";
                var success = true;
                var numSelected = _modsListViewModel.SimpleModsList.Where(m => m.ShouldExport).Count();
                if (shouldDelete && numSelected > 0)
                {
                    try
                    {
                        // TODO: After exporting, should I reset mods to ShouldExport = true?
                        outputPath = await _exportService.Export(_modsListViewModel.ModPack, type);
                        if (String.IsNullOrWhiteSpace(outputPath))
                        {
                            success = false;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        success = false;
                        _logService.Warning("Export was cancelled.");
                    }
                    catch (Exception ex)
                    {
                        success = false;
                        _logService.Error(ex, "Export Service threw an exception.");
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
                if (numSelected == 0)
                {
                    _logService.Information("Zero mods were selected for export.");
                }
                else if (!shouldDelete)
                {
                    _logService.Information("File was not written.");
                }
            }
            catch (Exception ex)
            {
                _logService.Error(ex, "Export failed");
            }
            finally
            {
                IsBusy = false;
                ExportCommand.RaiseCanExecuteChanged();
            }
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

        public bool CanExport => _modsListViewModel.CanExport && !IsBusy;

        private void DisplaySuccess(string path = "")
        {
            _messageBoxService.Show($"Finished exporting {path}", "Finished Export", MessageBoxButtons.OK);
        }

        private void DisplayFailure()
        {
            _messageBoxService.Show($"Failed to write ttmp2.", "", MessageBoxButtons.OK);
        }
    }
}
