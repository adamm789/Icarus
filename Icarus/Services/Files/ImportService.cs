using Icarus.Util;
using Icarus.ViewModels.Mods;
using Icarus.ViewModels.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.Models.DataContainers;

using Serilog;
using Icarus.ViewModels;
using Icarus.Services.Interfaces;
using Lumina;
using Icarus.Mods;
using Icarus.Mods.DataContainers;
using Icarus.Services.GameFiles;
using Lumina.Data;
using Lumina.Data.Files;
using xivModdingFramework.Materials.FileTypes;
using xivModdingFramework.Mods.DataContainers;

using ModPack = Icarus.Mods.DataContainers.ModPack;
using SixLabors.ImageSharp.PixelFormats;
using xivModdingFramework.Textures.FileTypes;
using Icarus.Mods.GameFiles;
using System.Windows.Forms;
using Icarus.Mods.Interfaces;
using System.Diagnostics;
using Icarus.Util.Extensions;
using System.Net.Http.Headers;
using Icarus.Services.GameFiles.Interfaces;

namespace Icarus.Services.Files
{
    public class ImportService : LuminaDependentServiceBase<ImportService>
    {
        readonly ConverterService _converterService;
        readonly ILogService _logService;
        readonly TexToolsModPackImporter _ttmpImporter;
        readonly IGameFileService _gameFileDataService;

        DirectoryInfo gameDirectoryFramework;

        protected Queue<string> _importFileQueue = new();

        public ImportService(IGameFileService gameFileDataService, SettingsService settingsService, ConverterService converterService, ILogService logService, LuminaService lumina) : base(lumina)
        {
            _logService = logService;
            _converterService = converterService;
            _gameFileDataService = gameFileDataService;

            var projectDirectory = settingsService.ProjectDirectory;

            var gamePathFramework = Path.Combine(settingsService.GameDirectoryLumina, "ffxiv");
            gameDirectoryFramework = new(gamePathFramework);
            _ttmpImporter = new(projectDirectory, gamePathFramework);
        }

        public bool IsImporting
        {
            get { return _importFileQueue.Count > 0; }
        }

        public string ImportingFile
        {
            get { return _importFileQueue.Count <= 0 ? String.Empty : _importFileQueue.Peek(); }
        }

        bool _isImportingAdvanced = false;
        public bool IsImportingAdvanced
        {
            get { return _isImportingAdvanced; }
            set { _isImportingAdvanced = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Returns null if import fails
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>A ModPack with SimpleModsList and potentially ModPackPages filled</returns>
        public async Task<ModPack> ImportFile(string filePath)
        {
            // TODO: ImportFile: return null on import fail? Or empty modPAck?
            var importingFile = "Importing: " + filePath;
            _logService.Information(importingFile);
            _importFileQueue.Enqueue(importingFile);
            UpdateUI();

            var ext = Path.GetExtension(filePath);
            var retModPack = new ModPack();
            if (ext == ".fbx")
            {
                retModPack = await ImportModel(filePath);
            }
            else if (ext == ".ttmp2")
            {
                retModPack = await ImportTTModPack(filePath);
            }
            else if (ext == ".dds")
            {
                // TODO: How to handle dds, which, I believe, can be both a texture or a material
                retModPack = await Task.Run(() => ImportColorset(filePath));
            }
            else if (ext == ".png" || ext == ".bmp")
            {
                retModPack = ImportTexture(filePath);
            }
            _importFileQueue.Dequeue();
            UpdateUI();

            _logService.Verbose("Returning modpack.");
            return retModPack;
        }

        public async Task<ModPack> ImportTTModPack(string filePath)
        {
            IsImportingAdvanced = true;
            try
            {
                var modPack = await ImportTexToolsModPack(filePath);
                _logService.Information($"Imported {modPack.SimpleModsList.Count} mods.");
                return modPack;
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"Could not import ttmp2: {filePath}");
            }
            finally
            {
                IsImportingAdvanced = false;
            }
            return new ModPack();
        }

        public async Task<ModPack> ImportModel(string filePath)
        {
            // Import File -> Convert to TTModel with converter.exe (TTModel Imported)
            // Choose destination item -> XivMdl OgMdl -> TTModel Original
            // WriteModelToBytes (Imported, OgMdl)
            // new Mdl(Imported, OgMdl).MakeNewMdlFile
            try
            {
                var importedModel = await _converterService.FbxToTTModel(filePath);
                var sane = TTModel.SanityCheck(importedModel, _logService.LoggingFunction);

                _logService.Information("Checking for common user errors.");
                TTModel.CheckCommonUserErrors(importedModel, _logService.LoggingFunction);

                var mod = new ModelMod(filePath, importedModel);

                var retVal = new ModPack();
                if (mod == null) return retVal;

                retVal.SimpleModsList.Add(mod);
                return retVal;
            }
            catch (InvalidOperationException ex)
            {
                //_logService.Error(ex, "Conversion did not succeed.");
                string err = "";
                err = int.Parse(ex.Message) switch
                {
                    500 => "Model is not triangulated.",
                    300 or 201 => "Sqlite Error.",
                    _ => "File is invalid.",
                };
                _logService.Error(ex, $"Conversion failed. {err}");
            }
            catch (Win32Exception ex)
            {
                _logService.Error(ex, "The folder /converters was not found.");
                //Log.Show("Please copy and paste the converters folder from TexTools into the same folder as the exe.");
            }

            _logService.Error($"Could not import {filePath} as model.");
            return new ModPack();
        }

        public ModPack ImportColorset(string filePath)
        {
            try
            {
                var directory = new DirectoryInfo(filePath);
                var colorsetData = Tex.GetColorsetDataFromDDS(directory);
                var colorsetDyeData = Tex.GetColorsetExtraDataFromDDS(directory);

                var materialMod = new MaterialMod(colorsetData, colorsetDyeData);

                var modPack = new ModPack();
                modPack.SimpleModsList.Add(materialMod);
                return modPack;
            } catch (Exception ex)
            {
                _logService.Error(ex, $"Could not get colorset data from {filePath}");
            }
            return new ModPack();
        }

        public async Task<ModPack> ImportTexToolsModPack(string filePath)
        {
            var retPack = await _ttmpImporter.ExtractTexToolsModPack(filePath);
                
            foreach (var mod in retPack.SimpleModsList)
            {
                CompleteMod(mod);
            }
            return retPack;
        }

        private void CompleteMod(IMod mod)
        {
            if (mod is ModelMod mdlMod)
            {
                var gameFile = _gameFileDataService.TryGetModelFileData(mdlMod.Path);
                if (gameFile == null)
                {
                    _logService.Error($"Could not get vanilla information of {mdlMod.Path}.");
                    _logService.Error("Skipping TTModel and XivMdl assignment.");
                    return;
                }

                mdlMod.TTModel = gameFile.TTModel;
                mdlMod.XivMdl = gameFile.XivMdl;
            }
        }

        public ModPack ImportTexture(string filePath)
        {
            var retPack = new ModPack();

            // TODO: TextureMod
            // seems like there's really nothing I can add?
            // Texture mods don't seem too editable
            
            var texMod = new TextureMod(false)
            {
                ModFileName = filePath,
                ModFilePath = filePath
            };
            retPack.SimpleModsList.Add(texMod);
            
            return retPack;
        }

        private void UpdateUI()
        {
            OnPropertyChanged(nameof(ImportingFile));
            OnPropertyChanged(nameof(IsImporting));
        }
    }
}
