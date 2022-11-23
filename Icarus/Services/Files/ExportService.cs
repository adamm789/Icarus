using Icarus.Mods.DataContainers;
using Icarus.Services.GameFiles;
using Icarus.Services.Interfaces;
using Icarus.Util;
using Icarus.Util.Export;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Icarus.Services.Files
{
    public class ExportService : LuminaDependentServiceBase<ExportService>
    {
        readonly ISettingsService _settingsService;
        readonly ConverterService _converterService;
        readonly ILogService _logService;
        protected string _outputDirectory;

        protected PenumbraExporter _penumbraExporter;
        protected TexToolsExporter _textoolsExporter;
        protected RawExporter _rawExporter;

        public ExportService(ILogService logService, ISettingsService settingsService, ConverterService converterService, LuminaService luminaService) : base(luminaService)
        {
            _settingsService = settingsService;
            _outputDirectory = _settingsService.OutputDirectory;
            _converterService = converterService;
            _logService = logService;
        }

        bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { _isBusy = value; OnPropertyChanged(); }
        }
        protected override void OnLuminaSet()
        {
            _rawExporter = new(_converterService, _lumina, _logService);
            _penumbraExporter = new(_lumina, _logService);
            _textoolsExporter = new(_lumina, _logService);
        }

        private void ReportProgress((int a, int b) pair)
        {
            _logService.Information($"{pair.a} - {pair.b}");
        }

        public async Task<string> Export(ModPack modPack, ExportType exportType, FileSystemInfo info, bool toPmp = true)
        {
            IsBusy = true;
            var progress = new Progress<(int, int)>(ReportProgress);

            try
            {
                switch (exportType)
                {
                    case ExportType.TexToolsSimple:
                        return await _textoolsExporter.ExportToSimple(modPack, (FileInfo)info);
                    case ExportType.TexToolsAdvanced:
                        return await _textoolsExporter.ExportToAdvanced(modPack, (FileInfo)info);
                    case ExportType.RawSimple:
                        return await _rawExporter.ExportToSimple(modPack, (DirectoryInfo)info);
                    case ExportType.RawAdvanced:
                        return await _rawExporter.ExportToAdvanced(modPack, (DirectoryInfo)info);
                }
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"Could not export modpack.");
                throw;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task<string> Export(ModPack modPack, ExportType exportType, bool toPmp = true)
        {
            IsBusy = true;
            var progress = new Progress<(int, int)>(ReportProgress);
            try
            {
                var file = Path.Combine(_outputDirectory, $"{modPack.Name}.ttmp2");

                switch (exportType)
                {
                    case ExportType.TexToolsSimple:
                        return await _textoolsExporter.ExportToSimple(modPack, new FileInfo(file), null, progress);
                    case ExportType.TexToolsAdvanced:
                        return await _textoolsExporter.ExportToAdvanced(modPack, new FileInfo(file));
                    case ExportType.PenumbraSimple:
                        return await _penumbraExporter.ExportToSimple(modPack, _outputDirectory, toPmp);
                    case ExportType.PenumbraAdvanced:
                        return await _penumbraExporter.ExportToAdvanced(modPack, _outputDirectory, toPmp);
                    case ExportType.RawSimple:
                        return await _rawExporter.ExportToSimple(modPack, new DirectoryInfo(_outputDirectory));
                    case ExportType.RawAdvanced:
                        return await _rawExporter.ExportToAdvanced(modPack, new DirectoryInfo(_outputDirectory));
                }
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"Could not export modpack.");
                throw;
            }
            finally
            {
                IsBusy = false;
            }
            return "";
        }
    }

    [Flags]
    public enum ExportType
    {
        TexToolsSimple = 1,
        TexToolsAdvanced = 2,
        PenumbraSimple = 4,
        PenumbraAdvanced = 8,
        RawSimple = 16,
        RawAdvanced = 32,

        Simple = TexToolsSimple | PenumbraSimple | RawSimple,
        Advanced = TexToolsAdvanced | PenumbraAdvanced | RawAdvanced,

        TexTools = TexToolsSimple | TexToolsAdvanced,
        Penumbra = PenumbraSimple | PenumbraAdvanced,
        Raw = RawSimple | RawAdvanced
    }
}
