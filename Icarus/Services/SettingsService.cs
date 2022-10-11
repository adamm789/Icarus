using Icarus.Services.Interfaces;
using System;
using System.Configuration;
using System.IO;

namespace Icarus.Services
{
    public class SettingsService : ServiceBase<SettingsService>, ISettingsService
    {
        private static readonly Properties.Settings DefaultSettings = Properties.Settings.Default;

        // TODO: Add log verbosity to settings (only to DEBUG)
        public SettingsService()
        {
            ProjectDirectory = GetProjectDirectory();
            ConverterFolder = Path.Combine(GetProjectDirectory(), "converters");

            if (String.IsNullOrWhiteSpace(BrowseDirectory))
            {
                BrowseDirectory = GetProjectDirectory();
                DefaultSettings.Save();
            }
            if (String.IsNullOrWhiteSpace(OutputDirectory))
            {
                OutputDirectory = Path.Combine(GetProjectDirectory(), "output");
                DefaultSettings.Save();
            }
        }

        public string ProjectDirectory { get; protected set; }
        public string ConverterFolder { get; protected set; }

        // TODO: Changing Lumina directories will not update until the next restart
        public string GameDirectoryLumina
        {
            get { return DefaultSettings.GameDirectoryLumina; }
            set { DefaultSettings.GameDirectoryLumina = value; OnPropertyChanged(); }
        }
        public bool AdvancedSettings
        {
            get { return DefaultSettings.AdvancedSettings; }
            set { DefaultSettings.AdvancedSettings = value; OnPropertyChanged(); }
        }
        public string BrowseDirectory
        {
            get { return DefaultSettings.BrowseDirectory; }
            set { DefaultSettings.BrowseDirectory = value; OnPropertyChanged(); }
        }
        public string OutputDirectory
        {
            get { return DefaultSettings.OutputDirectory; }
            set { DefaultSettings.OutputDirectory = value; OnPropertyChanged(); }
        }

        public event SettingsSavingEventHandler SettingsSaving
        {
            add { DefaultSettings.SettingsSaving += value; }
            remove { DefaultSettings.SettingsSaving -= value; }
        }

        public void Save()
        {
            DefaultSettings.Save();
        }


        public void LoadSettings()
        {

        }

        private static string GetProjectDirectory()
        {
#if DEBUG
            try
            {
                return new DirectoryInfo(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName).FullName;
            }
            catch (Exception ex) { }
#endif
            return Directory.GetCurrentDirectory() + "\\";
        }
    }
}
