using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.Services.Interfaces
{
    public interface ILogService : IServiceProvider
    {
        void Error(string s);
        void Warning(string s);
        void Information(string s);
        void Verbose(string s);

        void LoggingFunction(bool warning, string message);
    }
}
