using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.Services.Interfaces
{
    public interface ISettingsService : IServiceProvider
    {
        string GameDirectoryLumina { get; set; }
        public string ProjectDirectory { get; }
        public string ConverterFolder { get; }
        public bool AdvancedSettings { get; set; }
        public string BrowseDirectory { get; set; }
        public string OutputDirectory { get; set; }
    }
}
