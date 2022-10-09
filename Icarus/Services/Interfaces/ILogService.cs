using Icarus.UI;
using Serilog;
using System;
using System.IO;

namespace Icarus.Services.Interfaces
{
    public interface ILogService : IServiceProvider
    {
        void Verbose(string s);
        void Verbose(Exception ex, string s = "");
        void Debug(string s);
        void Debug(Exception ex, string s = "");
        void Information(string s);
        void Warning(string s);
        void Warning(Exception ex, string s = "");
        void Error(string s);
        void Error(Exception ex, string s = "");
        void Fatal(string s);
        void Fatal(Exception ex, string s = "");

        ILogger Logger { get; }
        StringWriter StringWriter { get; }
        InMemorySink Sink { get; }

        void LoggingFunction(bool warning, string message);
    }
}
