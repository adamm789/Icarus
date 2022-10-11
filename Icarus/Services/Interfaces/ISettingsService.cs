using System;
using System.ComponentModel;
using System.Configuration;

namespace Icarus.Services.Interfaces
{
    public interface ISettingsService : IServiceProvider, INotifyPropertyChanged
    {
        string GameDirectoryLumina { get; set; }
        string ProjectDirectory { get; }
        string ConverterFolder { get; }
        bool AdvancedSettings { get; set; }
        string BrowseDirectory { get; set; }
        string OutputDirectory { get; set; }
        event SettingsSavingEventHandler SettingsSaving;
        void Save();
    }
}
