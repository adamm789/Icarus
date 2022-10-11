using Icarus.Services.Interfaces;
using Icarus.UI;
using Serilog;
using Serilog.Events;
using System;
using System.IO;

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
        public void Warning(Exception ex, string s = "") => Log.Warning(ex, s);
        /*
        public void Warning(Exception ex, string s = "")
        {
            if (!String.IsNullOrWhiteSpace(s))
            {
                Log.Warning(s);
            }
            Log.Debug(ex, "");
        }
        */
        public void Error(string s) => Log.Error(s);
        public void Error(Exception ex, string s = "") => Log.Error(ex, s);
        /*
        public void Error(Exception ex, string s = "")
        {
            if (!String.IsNullOrWhiteSpace(s))
            {
                Log.Error(s);
            }
            Log.Debug(ex, "");
        }
        */
        public void Fatal(string s) => Log.Fatal(s);
        public void Fatal(Exception ex, string s = "") => Log.Fatal(ex, s);
        public ILogger Logger => Log.Logger;
        public StringWriter StringWriter { get; }
        public LogSink Sink { get; }


        readonly ISettingsService _settingsService;

        public LogService(ISettingsService settingsService)
        {

            _settingsService = settingsService;

            var projectDirectory = _settingsService.ProjectDirectory;
            var logPath = Path.Combine(projectDirectory, "logs/logs.txt");
            var verbosePath = Path.Combine(projectDirectory, "logs/verbose.txt");
            Sink = new LogSink();
            StringWriter = new();
            var outputTemplate = "{Timestamp:MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
#if DEBUG
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.File(verbosePath, rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true, retainedFileCountLimit: 7)
                .WriteTo.Logger(l => l.Filter.ByExcluding(e => e.Level == LogEventLevel.Verbose)
                .WriteTo.Debug()
                .WriteTo.Logger(l => l.Filter.ByExcluding(e => e.Level == LogEventLevel.Debug)
                .WriteTo.File(path: logPath, rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true, retainedFileCountLimit: 7,
                outputTemplate: outputTemplate)
                .WriteTo.Sink(Sink)))
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
