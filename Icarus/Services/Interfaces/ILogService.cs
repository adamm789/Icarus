using System;

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

        void LoggingFunction(bool warning, string message);
    }
}
