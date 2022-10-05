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

namespace Icarus.Services.Files
{
    public class ImportService : ServiceBase<ImportService>
    {
        readonly ConverterService _converterService;
        readonly ILogService _logService;
        readonly TexToolsModPackImporter _ttmpImporter;
        readonly IGameFileService _gameFileDataService;

        DirectoryInfo gameDirectoryFramework;

        protected Queue<string> _importFileQueue = new();

        public ImportService(IGameFileService gameFileDataService, SettingsService settingsService, ConverterService converterService, ILogService logService)
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
        /// <returns>A ModPack with SimpleModsList and ModPackPages potentially filled</returns>
        public async Task<ModPack?> ImportFile(string filePath)
        {
            var importingFile = "Importing: " + filePath;
            _logService.Information(importingFile);
            _importFileQueue.Enqueue(importingFile);
            UpdateUI();

            var file = new FileInfo(filePath);
            var retModPack = new ModPack();
            if (file.Extension == ".fbx")
            {
                var model = await ImportModel(filePath);
                if (model != null)
                {
                    retModPack.SimpleModsList.Add(model);
                }
                else
                {
                    retModPack = null;
                }
            }
            else if (file.Extension == ".ttmp2")
            {
                retModPack = await ImportTTModPack(filePath);
            }
            else if (file.Extension == ".dds")
            {
                // TODO: How to handle dds, which, I believe, can be both a texture or a material
                retModPack = await ImportColorset(filePath);
            }
            else if (file.Extension == ".png")
            {
                retModPack = ImportTexture(filePath);
            }
            else if (file.Extension == ".mdl")
            {

            }
            _importFileQueue.Dequeue();
            UpdateUI();

            _logService.Verbose("Returning modpack.");
            return retModPack;
        }

        public async Task<ModelMod?> ImportModel(string filePath)
        {
            try
            {
                var importedModel = await _converterService.FbxToTTModel(filePath);
                var sane = TTModel.SanityCheck(importedModel, _logService.LoggingFunction);

                _logService.Information("Checking for common user errors.");
                TTModel.CheckCommonUserErrors(importedModel, _logService.LoggingFunction);

                return new ModelMod(filePath, importedModel);
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

            _logService.Error($"Could not import {filePath}.");
            return null;
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
                _logService.Error(ex);
            }
            finally
            {
                IsImportingAdvanced = false;
            }
            return new ModPack();
        }

        public async Task<ModPack> ImportColorset(string filePath)
        {
            try
            {
                var modPack = new ModPack();
                var directory = new DirectoryInfo(filePath);
                var colorsetData = Tex.GetColorsetDataFromDDS(directory);
                var colorsetDyeData = Tex.GetColorsetExtraDataFromDDS(directory);
                /*
                using (var br = new BinaryReader(new FileStream(filePath, FileMode.Open)))
                {
                    var bytes = new byte[br.BaseStream.Length];
                    br.Read(bytes);
                    var xivMtrl = await MtrlExtensions.GetMtrlData(gameDirectoryFramework, bytes, "");
                    var materialMod = new MaterialMod(xivMtrl);
                }
                */
                var materialMod = new MaterialMod(colorsetData, colorsetDyeData);
                modPack.SimpleModsList.Add(materialMod);
                return modPack;
            } catch (Exception ex)
            {
                _logService.Error(ex);
            }
            return new ModPack();
        }

        public async Task<ModPack> ImportTexToolsModPack(string filePath)
        {
            var retPack = await _ttmpImporter.ExtractTexToolsModPack(filePath)
                /*
                .ContinueWith((t) =>
            {
                if (t.IsFaulted) throw t.Exception.InnerException;
                return t.Result;
            })
                */;
                

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
                var gameFile = _gameFileDataService.GetModelFileData(mdlMod.Path);
                if (gameFile == null)
                {
                    _logService.Error($"Could not get vanilla information of {mdlMod.Path}.");
                    _logService.Error("Skipping TTModel and XivMdl assignment.");
                    return;
                }

                mdlMod.TTModel = gameFile.TTModel;
                mdlMod.XivMdl = gameFile.XivMdl;
            }
            else if (mod is MaterialMod mtrlMod)
            {
                // TODO: Material mods are already complete...?
                //var gameFile = _gameFileDataService.GetMaterialFileData(mtrlMod.Path);
                //if (gameFile == null) return;
            }
        }

        public ModPack ImportTexture(string filePath)
        {
            var retPack = new ModPack();

            // TODO: TextureMod
            // seems like there's really nothing I can add?
            // Texture mods don't seem too editable
            /*
            var texMod = new TextureMod()
            {
                ModFileName = filePath,
                ModFilePath = filePath
            };
            retPack.SimpleModsList.Add(texMod);
            */
            return retPack;
        }

        private void UpdateUI()
        {
            OnPropertyChanged(nameof(ImportingFile));
            OnPropertyChanged(nameof(IsImporting));
        }
    }
}
