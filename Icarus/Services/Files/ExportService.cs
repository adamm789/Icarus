﻿using Icarus.Mods;
using Icarus.Mods.DataContainers;
using Icarus.Services.GameFiles;
using Icarus.Services.Interfaces;
using Icarus.Util;
using Icarus.Util.Export;
using Icarus.ViewModels.Mods;
using Icarus.ViewModels.Util;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using xivModdingFramework.Models.DataContainers;
using xivModdingFramework.Models.FileTypes;

namespace Icarus.Services.Files
{
    public class ExportService : LuminaDependentServiceBase<ExportService>
    {
        readonly SettingsService _settingsService;
        readonly ConverterService _converterService;
        readonly ILogService _logService;
        protected string _outputDirectory;

        protected PenumbraExporter _penumbraExporter;
        protected TexToolsExporter _textoolsExporter;
        protected RawExporter _rawExporter;

        public ExportService(ILogService logService, SettingsService settingsService, ConverterService converterService, LuminaService luminaService) : base(luminaService)
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
            base.OnLuminaSet();
            _rawExporter = new(_converterService, _lumina, _logService);
            _penumbraExporter = new(_logService);
            _textoolsExporter = new(_lumina, _logService);
        }

        public string GetOutputPath(ModPack modPack, ExportType exportType)
        {
            switch (exportType)
            {
                case ExportType.TexToolsSimple:
                case ExportType.TexToolsAdvanced:
                    return _textoolsExporter.GetOutputPath(modPack, _outputDirectory);
                case ExportType.PenumbraSimple:
                case ExportType.PenumbraAdvanced:
                    return _penumbraExporter.GetOutputPath(modPack, _outputDirectory);
                case ExportType.RawSimple:
                case ExportType.RawAdvanced:
                    return _rawExporter.GetOutputPath(modPack, _outputDirectory);
                default:
                    return "";
            }
        }

        public async Task<string> Export(ModPack modPack, ExportType exportType, bool toPmp = true)
        {
            IsBusy = true;
            try
            {
                switch (exportType)
                {
                    case ExportType.TexToolsSimple:
                        return await _textoolsExporter.ExportToSimple(modPack, _outputDirectory);
                    case ExportType.TexToolsAdvanced:
                        return await _textoolsExporter.ExportToAdvanced(modPack, _outputDirectory);
                    case ExportType.PenumbraSimple:
                        return await _penumbraExporter.ExportToSimple(modPack, _outputDirectory, toPmp);
                    case ExportType.PenumbraAdvanced:
                        return await _penumbraExporter.ExportToAdvanced(modPack, _outputDirectory, toPmp);
                    case ExportType.RawSimple:
                        return await _rawExporter.ExportToSimple(modPack, _outputDirectory);
                    case ExportType.RawAdvanced:
                        return await _rawExporter.ExportToAdvanced(modPack, _outputDirectory);
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

    public enum ExportType
    {
        TexToolsSimple,
        TexToolsAdvanced,
        PenumbraSimple,
        PenumbraAdvanced,

        RawSimple,
        RawAdvanced
    }
}
