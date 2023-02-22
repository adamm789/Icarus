﻿using Icarus.Mods;
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
using Ionic.Zip;

using IcarusModPack = Icarus.Mods.DataContainers.ModPack;
using xivModdingFramework.Mods.DataContainers;
using Newtonsoft.Json;
using System.Linq;
using TeximpNet.DDS;
using xivModdingFramework.Models.FileTypes;
using Lumina.Data;
using xivModdingFramework.Materials.FileTypes;

namespace Icarus.Services.Files
{
    public class ImportService : LuminaDependentServiceBase<ImportService>
    {
        readonly ConverterService _converterService;
        readonly ILogService _logService;
        readonly IGameFileService _gameFileDataService;
        readonly ISettingsService _settingsService;
        readonly IModelFileService _modelFileService;
        readonly IItemListService _itemListService;

        TexToolsModPackImporter _ttmpImporter;
        PenumbraModPackImporter _pmpImporter;

        protected Queue<string> _importFileQueue = new();

        public ObservableQueue<string> _stringQueue = new();

        ObservableCollection<IcarusModPack> ImportCollection;

        public ImportService(IGameFileService gameFileDataService, ISettingsService settingsService, ConverterService converterService, ILogService logService, LuminaService lumina,
            IModelFileService modelFileService, IItemListService itemListService) : base(lumina)
        {
            _settingsService = settingsService;
            _logService = logService;
            _converterService = converterService;
            _gameFileDataService = gameFileDataService;
            _modelFileService = modelFileService;
            _itemListService = itemListService;
        }

        protected override void OnLuminaSet()
        {
            var projectDirectory = _settingsService.ProjectDirectory;
            var gamePathFramework = Path.Combine(_settingsService.GameDirectoryLumina, "ffxiv");
            _ttmpImporter = new(projectDirectory, gamePathFramework);
            _pmpImporter = new(_settingsService.GameDirectoryLumina);
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

        public bool IsValidPenumbraDirectory(string dirPath) => _pmpImporter.IsValidPenumbraDirectory(new DirectoryInfo(dirPath));

        public async Task<IcarusModPack> ImportDirectory(string dirPath)
        {
            _logService?.Verbose($"Trying to import penumbra directory: {dirPath}");
            var importingFile = $"Importing {_importFileQueue.Count + 1} mod(s)";
            _importFileQueue.Enqueue(importingFile);

            UpdateProperties();
            var attr = File.GetAttributes(dirPath);
            if (attr.HasFlag(FileAttributes.Directory))
            {
                // check for default_mod.json and... group_ .json files
                var dir = new DirectoryInfo(dirPath);
                var modPack = await _pmpImporter.ImportPenumbraDirectory(dir);
                _logService?.Verbose($"Finished import");

                if (modPack != null)
                {
                    _logService?.Verbose($"Penumbra directory sucessfully imported {modPack.SimpleModsList.Count} mods");
                    _importFileQueue.Dequeue();

                    UpdateProperties();
                    return modPack;
                }
            }
            else
            {
                _logService?.Verbose($"{dirPath} could not be imported as directory.");
            }
            _importFileQueue.Dequeue();

            UpdateProperties();
            return new IcarusModPack();
        }

        /// <summary>
        /// Imports the file, filling a <see cref="ModPack"/> with mods and potentially <see cref="ModPack.ModPackPages"/>
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>The filled <see cref="ModPack"/> if successful. An empty <see cref="ModPack"/> otherwise.</returns>
        public async Task<IcarusModPack> ImportFile(string filePath)
        {
            var importingFile = $"Importing {_importFileQueue.Count + 1} mod(s)";
            _importFileQueue.Enqueue(importingFile);
            UpdateProperties();

            var ext = Path.GetExtension(filePath);
            var retModPack = new IcarusModPack();
            IMod? mod = null;
            switch (ext)
            {
                case ".fbx":
                    mod = await TryImportModel(filePath);
                    break;
                case ".ttmp2":
                    retModPack = await TryImportTTModPack(filePath);
                    break;
                case ".dds":
                    mod = await Task.Run(() => TryImportDDS(filePath));
                    break;
                case ".png":
                case ".bmp":
                    mod = ImportTexture(filePath);
                    break;
                case ".pmp":
                    retModPack = await _pmpImporter.ExtractPenumbraModPack(filePath);
                    break;
                case ".mdl":
                    mod = await Task.Run(() => _pmpImporter.TryImportMdl(filePath));
                    break;
                default:
                    _logService.Error($"Unsupported extension: {ext}");
                    break;
            }

            _importFileQueue.Dequeue();
            UpdateProperties();

            _logService.Verbose("Returning modpack.");

            if (retModPack == null)
            {
                return new IcarusModPack();
            }
            if (mod != null)
            {
                retModPack.SimpleModsList.Add(mod);
            }
            return retModPack;
        }

        public async Task<IcarusModPack?> TryImportTTModPack(string filePath)
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
            return new IcarusModPack();
        }

        const float minAcceptableSize = 0.5f;
        const float maxAcceptableSize = 2.0f;

        public async Task<ModelMod?> TryImportModel(string filePath)
        {
            // Import File -> Convert to TTModel with converter.exe (TTModel Imported)
            // Choose destination item -> XivMdl OgMdl -> TTModel Original
            // WriteModelToBytes (Imported, OgMdl)
            // new Mdl(Imported, OgMdl).MakeNewMdlFile
            try
            {
                _logService.Information($"Trying to import {filePath} as model.");
                var importedModel = await _converterService.FbxToTTModel(filePath).ContinueWith(
                    task =>
                    {
                        if (task.Exception != null)
                        {
                            var ex = task.Exception.InnerException;
                            throw ex;
                        }
                        return task.Result;
                    }
                    );
                var sane = TTModel.SanityCheck(importedModel, _logService.LoggingFunction);

                if (!sane)
                {
                    _logService.Error("Model was deemed insane.");
                    return null;
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
                    _logService.Debug($"Model is probably too small: {size}");
                }
                if (size > maxAcceptableSize)
                {
                    _logService.Debug($"Model is probably too big: {size}");
                }

                _logService.Information("Checking for common user errors.");
                TTModel.CheckCommonUserErrors(importedModel, _logService.LoggingFunction);

                var mod = new ModelMod(filePath, importedModel, ImportSource.Raw)
                {
                    ModFileName = filePath
                };
                _logService.Information($"Successfully imported model.");
                return mod;
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

        public IMod? TryImportDDS(string filePath)
        {
            var materialMod = TryImportColorSet(filePath);
            if (materialMod == null)
            {
                var textureMod = ImportTexture(filePath);
                return textureMod;
            }
            else
            {
                return materialMod;
            }
        }

        /// <summary>
        /// Tries to import a dds file and convert it into a colorset
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>A <see cref="MaterialMod">MaterialMod</see>, if successful. Null, otherwise.</returns>
        public MaterialMod? TryImportColorSet(string filePath)
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
                return materialMod;
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"Could not get colorset data from {filePath}");
            }
            return null;
        }

        public TextureMod? ImportTexture(string filePath)
        {
            _logService.Information($"Trying to import {filePath} as texture.");

            // TODO: Some sort of check for the file?
            // TODO: Store some actual data as opposed to just the file path?
            var texMod = new TextureMod(ImportSource.Raw)
            {
                ModFileName = filePath,
                ModFilePath = filePath,
            };
            return texMod;
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
                var gameFile = _modelFileService.TryGetModelFileData(mdlMod.Path);
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
