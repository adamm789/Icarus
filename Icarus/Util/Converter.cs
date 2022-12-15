using Icarus.Mods;
using Icarus.Services.Interfaces;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using xivModdingFramework.Cache;
using xivModdingFramework.General.Enums;
using xivModdingFramework.Models.DataContainers;

namespace Icarus.Util
{
    /// <summary>
    /// Class that runs "converter.exe" via a queue to prevent concurrent calls
    /// </summary>
    public class Converter
    {
        string _converterFolder;
        string _gameDirectory;
        string _fbxFolder;
        private TaskQueue _taskQueue;
        DirectoryInfo _gameDirectoryInfo;
        ILogService _logService;

        public Converter(string converterFolder, string gameDirectory, ILogService logService)
        {
            _converterFolder = converterFolder;
            _gameDirectory = gameDirectory;
            _logService = logService;

            _fbxFolder = Path.Combine(_converterFolder, "fbx");
            _taskQueue = new();

            _gameDirectoryInfo = new DirectoryInfo(Path.Combine(_gameDirectory, "ffxiv"));

            // Needed for ttmodel.SaveToFile
            XivCache.SetGameInfo(_gameDirectoryInfo, XivLanguage.English, 11, false, false, null, true);
        }

        public async Task<TTModel?> FbxToTTModel(string filePath)
        {
            return await _taskQueue.Enqueue(() => Task.Run(() =>
            {
                var file = new FileInfo(filePath);
                if (file.Extension == ".fbx")
                {
                    var dbPath = Path.Combine(_fbxFolder, "result.db");
                    if (File.Exists(dbPath))
                    {
                        File.Delete(dbPath);
                    }
                    ConverterProcess(filePath, _fbxFolder);

                    return TTModel.LoadFromFile(dbPath);
                }
                else
                {
                    return null;
                }
            }));
        }

        public async Task TTModelToFbx(TTModel model, DirectoryInfo outputDirectory, string outputFileName = "")
        {
            await _taskQueue.Enqueue(() => Task.Run(() =>
            {
                var dbPath = Path.Combine(_fbxFolder, "input.db");

                if (File.Exists(dbPath))
                {
                    // Seems to get an IOException without these two lines
                    // File is being used by another process
                    System.GC.Collect();
                    System.GC.WaitForPendingFinalizers();
                    File.Delete(dbPath);
                }

                var modelName = model.Source;

                // unnecessary?
                // model.SetFullModelDBMetaData(dbPath, modelName);

                var outputPath = outputDirectory.FullName;

                string outputFilePath = "";
                if (String.IsNullOrWhiteSpace(outputFileName))
                {
                    outputFilePath = Path.Combine(outputPath, "result.fbx");
                    var num = 0;
                    while (File.Exists(outputFilePath))
                    {
                        outputFilePath = Path.Combine(outputPath, "result (" + num + ").fbx");
                        num++;
                    }
                }
                else
                {
                    var num = 0;
                    outputFilePath = Path.Combine(outputPath, $"{outputFileName}.fbx");
                    while (File.Exists(outputFilePath))
                    {
                        outputFilePath = Path.Combine(outputPath, outputFileName + " (" + num + ").fbx");
                        num++;
                    }
                }
                var _useAllBones = false;
                model.SaveToFile(dbPath, useAllBones: _useAllBones);

                Log.Verbose($"Starting converter on {dbPath}.");
                ConverterProcess(dbPath, _fbxFolder);
                Log.Verbose("Converter finished.");

                var outputFile = Path.Combine(_fbxFolder, "result.fbx");

                File.Move(outputFile, outputFilePath);
            }));
        }

        private void ConverterProcess(string filePath, string converterFolder)
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = converterFolder + "\\converter.exe",
                    Arguments = "\"" + filePath + "\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    WorkingDirectory = "" + converterFolder + "",
                    CreateNoWindow = true
                }
            };

            proc.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
            {
                _logService.Information(e.Data);
            };

            proc.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
            {
                _logService.Error(e.Data);
            };

            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
            proc.WaitForExit();

            if (proc.ExitCode != 0)
            {
                throw new InvalidOperationException(proc.ExitCode.ToString());
            }
        }
    }
}
