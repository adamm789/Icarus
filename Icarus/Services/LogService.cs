﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.DependencyInjection;
using Icarus.Services.Interfaces;
using Serilog;
using Serilog.Events;

namespace Icarus.Services
{
    public class LogService : ServiceBase<LogService>, ILogService
    {
        public void Verbose(string s) => Log.Verbose(s);
        public void Verbose(Exception ex, string s = "") => Log.Verbose(ex, s);
        public void Debug(string s) => Log.Debug(s);
        public void Debug(Exception ex, string s = "") => Log.Debug(ex, s);
        public void Information(string s) => Log.Information(s);
        public void Warning(string s) => Log.Warning(s);
        //public void Warning(Exception ex, string s = "") => Log.Warning(ex, s);
        public void Warning(Exception ex, string s = "")
        {
            if (!String.IsNullOrWhiteSpace(s))
            {
                Log.Warning(s);
            }
            Log.Debug(ex, "");
        }
        public void Error(string s) => Log.Error(s);
        //public void Error(Exception ex, string s = "") => Log.Error(ex, s);
        public void Error(Exception ex, string s = "")
        {
            if (!String.IsNullOrWhiteSpace(s))
            {
                Log.Error(s);
            }
            Log.Debug(ex, "");
        }
        public void Fatal(string s) => Log.Fatal(s);
        public void Fatal(Exception ex, string s = "") => Log.Fatal(ex, s);


        readonly SettingsService _settingsService;

        public LogService(SettingsService settingsService)
        {
            _settingsService = settingsService;

            var projectDirectory = _settingsService.ProjectDirectory;
            var logPath = Path.Combine(projectDirectory, "logs/logs.txt");
            var verbosePath = Path.Combine(projectDirectory, "logs/verbose.txt");

#if DEBUG
            // TODO: Filter into different txt files

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.File(verbosePath, rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true, retainedFileCountLimit: 7)
                .WriteTo.Logger(l => l.Filter.ByExcluding(e => e.Level == LogEventLevel.Verbose)
                .WriteTo.Debug()
                .WriteTo.Logger(l => l.Filter.ByExcluding(e => e.Level == LogEventLevel.Debug)
                .WriteTo.File(path: logPath, rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true, retainedFileCountLimit: 7)))
                .CreateLogger();
#else
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information().WriteTo.File(logPath, rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true, retainedFileCountLimit: 7)
                .CreateLogger();
#endif

            Log.Information("=== Logger created. ===");
            Log.Debug("Debug");
            Log.Verbose("Verbose");
        }

        public void LoggingFunction(bool warning, string message)
        {
            if (warning)
            {
                Log.Warning(message);
            }
            else
            {
                Log.Information(message);
            }
        }
    }
}
