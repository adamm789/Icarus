using Icarus.Mods;
using Icarus.Services.GameFiles;
using Icarus.Services.Interfaces;
using Icarus.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.Models.DataContainers;
using xivModdingFramework.Models.FileTypes;

namespace Icarus.Services.Files
{
    /// <summary>
    /// Class that specifically runs "converter.exe"
    /// </summary>
    public class ConverterService : LuminaDependentServiceBase<ConverterService>
    {
        string _converterFolder;
        string _gameDirectory;
        private Converter _converter;
        readonly SettingsService _settings;
        readonly ILogService _logService;
        public ConverterService(SettingsService settings, LuminaService luminaService, ILogService logService) : base (luminaService)
        {
            _settings = settings;
            _logService = logService;
            _converterFolder = _settings.ConverterFolder;
            _gameDirectory = _settings.GameDirectoryLumina;
        }

        public async Task<TTModel?> FbxToTTModel(string filePath)
        {
            return await _converter.FbxToTTModel(filePath);
        }

        public async Task ModelModToFbx(ModelMod mod, DirectoryInfo outputDirectory, string outputFileName = "")
        {
            await _converter.ModelModToFbx(mod, outputDirectory, outputFileName);
        }

        protected override void OnLuminaSet()
        {
            base.OnLuminaSet();
            _gameDirectory = _settings.GameDirectoryLumina;
            _converter = new(_converterFolder, _gameDirectory);
        }
    }
}
