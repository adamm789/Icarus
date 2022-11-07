using Icarus.Mods;
using Icarus.Mods.Interfaces;
using Icarus.Services.GameFiles;
using Icarus.Services.GameFiles.Interfaces;
using Icarus.Services.Interfaces;
using Icarus.Util;
using Icarus.Util.Extensions;
using Icarus.ViewModels.Util;
using Lumina.Data.Files;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using Icarus.Util.Import;
using xivModdingFramework.Models.DataContainers;
using xivModdingFramework.Textures.FileTypes;
using System.Collections.ObjectModel;
using Icarus.ViewModels.Import;
using Icarus.Mods.DataContainers;

namespace Icarus.Services.Files
{
    public class ImportService : LuminaDependentServiceBase<ImportService>
    {
        readonly ConverterService _converterService;
        readonly ILogService _logService;
        readonly IGameFileService _gameFileDataService;
        readonly ISettingsService _settingsService;

        TexToolsModPackImporter _ttmpImporter;
        protected Queue<string> _importFileQueue = new();

        public ObservableQueue<string> _stringQueue = new();

        ObservableCollection<ModPack> ImportCollection;

        public ImportService(IGameFileService gameFileDataService, ISettingsService settingsService, ConverterService converterService, ILogService logService, LuminaService lumina) : base(lumina)
        {
            _settingsService = settingsService;
            _logService = logService;
            _converterService = converterService;
            _gameFileDataService = gameFileDataService;
        }

        protected override void OnLuminaSet()
        {
            var projectDirectory = _settingsService.ProjectDirectory;
            var gamePathFramework = Path.Combine(_settingsService.GameDirectoryLumina, "ffxiv");
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
        /// Imports the file, filling a <see cref="ModPack"/> with mods and potentially <see cref="ModPack.ModPackPages"/>
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>The filled <see cref="ModPack"/> if successful. An empty <see cref="ModPack"/> otherwise.</returns>
        public async Task<ModPack?> ImportFile(string filePath)
        {
            var importingFile = "Importing: " + filePath;
            _importFileQueue.Enqueue(importingFile);
            UpdateProperties();

            var ext = Path.GetExtension(filePath);
            ModPack? retModPack = null;
            if (ext == ".fbx")
            {
                retModPack = await TryImportModel(filePath);
            }
            else if (ext == ".ttmp2")
            {
                retModPack = await TryImportTTModPack(filePath);
            }
            else if (ext == ".dds")
            {
                retModPack = await Task.Run(() => TryImportDDS(filePath));
            }
            else if (ext == ".png" || ext == ".bmp")
            {
                retModPack = ImportTexture(filePath);
            }
            else if (ext == ".mdl")
            {
                retModPack = TryImportRawModel(filePath);
            }
            _importFileQueue.Dequeue();
            UpdateProperties();

            _logService.Verbose("Returning modpack.");
            return retModPack;
        }

        public async Task<ModPack?> TryImportTTModPack(string filePath)
        {
            IsImportingAdvanced = true;
            try
            {
                _logService.Information($"Trying to import {filePath} as ttmp2.");
                var modPack = await _ttmpImporter.ExtractTexToolsModPack(filePath);

                foreach (var mod in modPack.SimpleModsList)
                {
                    CompleteMod(mod);
                }
                _logService.Information($"Success. Imported {modPack.SimpleModsList.Count} mods.");
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
            return null;
        }

        const float minAcceptableSize = 0.5f;
        const float maxAcceptableSize = 2.0f;

        public async Task<ModPack?> TryImportModel(string filePath)
        {
            // Import File -> Convert to TTModel with converter.exe (TTModel Imported)
            // Choose destination item -> XivMdl OgMdl -> TTModel Original
            // WriteModelToBytes (Imported, OgMdl)
            // new Mdl(Imported, OgMdl).MakeNewMdlFile
            try
            {
                // TODO: "Missing" "Original" shape data?
                _logService.Information($"Trying to import {filePath} as model.");
                var importedModel = await _converterService.FbxToTTModel(filePath);
                var sane = TTModel.SanityCheck(importedModel, _logService.LoggingFunction);

                if (!sane)
                {
                    _logService.Error("Model was deemed insane. Model has no data.");
                }

                // TODO: Check size of model
                var size = importedModel.GetModelSize();
                /*
                 * From https://github.com/TexTools/FFXIV_TexTools_UI/blob/37290b2897c79dd1e913bb4ff90285f0e620ca9d/FFXIV_TexTools/ViewModels/ImportModelEditViewModel.cs#L206
                 * 
                 * minAcceptableSize = 0.5f
                 * MaxAcceptableSize = 2.0f
                 * 
                 * if (size < minAcceptableSize * oldModelSize)
                 * 
                 * if (size > maxAcceptableSize * oldModelSize)
                 */

                if (size < minAcceptableSize)
                {
                    _logService.Warning($"Model is probably too small: {size}");
                }
                if (size > maxAcceptableSize)
                {
                    _logService.Warning($"Model is probably too big: {size}");
                }

                _logService.Information("Checking for common user errors.");
                TTModel.CheckCommonUserErrors(importedModel, _logService.LoggingFunction);

                var mod = new ModelMod(filePath, importedModel, ImportSource.Raw)
                {
                    ModFileName = filePath
                };

                var retVal = new ModPack();
                if (mod == null) return retVal;

                _logService.Information($"Successfully imported model.");
                retVal.SimpleModsList.Add(mod);
                return retVal;
            }
            catch (InvalidOperationException ex)
            {
                string err = "";
                err = int.Parse(ex.Message) switch
                {
                    500 => "Model is not triangulated.",
                    300 or 201 => "Sqlite Error.",
                    _ => "File is invalid.",
                };
                _logService.Error(ex, $"Import failed. {err}");
            }
            catch (Win32Exception ex)
            {
                _logService.Fatal(ex, "The folder /converters was not found.");
            }

            _logService.Error($"Could not import {filePath} as model.");
            return null;
        }

        public ModPack? TryImportRawModel(string filePath)
        { 
            // TODO: ImportRawModel?
            try
            {
                var model = _lumina.GetFileFromDisk<MdlFile>(filePath);
                if (model != null)
                {
                    var mldWithLumina = new MdlWithLumina(model);
                }
            }
            catch(Exception ex)
            {
                _logService.Error(ex, "An exception has occurred.");
            }
            return null;
        }

        private ModPack? TryImportDDS(string filePath)
        {
            var modPack = TryImportColorSet(filePath);
            if (modPack == null)
            {
                modPack = ImportTexture(filePath);
            }
            return modPack;
        }

        /// <summary>
        /// Tries to import a dds file and convert it into a colorset
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>A <see cref="ModPack">ModPack</see> with a single colorset mod, if successful. An empty modpack otherwise.</returns>
        public ModPack? TryImportColorSet(string filePath)
        {
            try
            {
                _logService.Information($"Trying to import {filePath} as colorset/material.");
                var directory = new DirectoryInfo(filePath);
                var colorsetData = Tex.GetColorsetDataFromDDS(directory);
                var colorsetDyeData = Tex.GetColorsetExtraDataFromDDS(directory);

                var materialMod = new MaterialMod(colorsetData, colorsetDyeData)
                {
                    ModFileName = filePath,
                    ModFilePath = filePath
                };

                var modPack = new ModPack();
                modPack.SimpleModsList.Add(materialMod);
                return modPack;
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"Could not get colorset data from {filePath}");
            }
            return null;
        }

        public ModPack? ImportTexture(string filePath)
        {
            _logService.Information($"Trying to import {filePath} as texture.");

            // TODO: Some sort of check for the file?
            // TODO: Store some actual data as opposed to just the file path?
            var retPack = new ModPack();
            var texMod = new TextureMod(ImportSource.Raw)
            {
                ModFileName = filePath,
                ModFilePath = filePath,
            };
            retPack.SimpleModsList.Add(texMod);

            return retPack;
        }

        /// <summary>
        /// Fills out the rest of any mod's "FileData"
        /// Currently only changes ModelMods
        /// </summary>
        /// <param name="mod"></param>
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

        private void UpdateProperties()
        {
            OnPropertyChanged(nameof(ImportingFile));
            OnPropertyChanged(nameof(IsImporting));
        }
    }
}
