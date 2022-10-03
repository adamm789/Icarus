using Icarus.Services;
using Icarus.Services.Interfaces;
using Icarus.Services.UI;
using Icarus.ViewModels.Util;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace Icarus.ViewModels
{
    internal class AppSettingsViewModel : NotifyPropertyChanged
    {
        readonly char _separator = Path.DirectorySeparatorChar;
        readonly SettingsService _settings;
        readonly IMessageBoxService _messageBox;
        public AppSettingsViewModel(SettingsService settings, IMessageBoxService messageBox)
        {
            _settings = settings;
            _messageBox = messageBox;
        }

        public bool AdvancedSettings
        {
            get { return _settings.AdvancedSettings; }
            set
            {
                if (_settings.AdvancedSettings != value)
                {
                    _settings.AdvancedSettings = value;
                    OnPropertyChanged();
                }
            }
        }

        DelegateCommand _setGamePathCommand;
        public DelegateCommand SetGamePathCommand
        {
            get { return _setGamePathCommand ??= new DelegateCommand(o => SetGamePath(o)); }
        }

        DelegateCommand _setDirectoryCommand;
        public DelegateCommand SetDirectoryCommand
        {
            get { return _setDirectoryCommand ??= new DelegateCommand(o => SetDirectory(o)); }
        }

        DelegateCommand _saveSettingsCommand;
        public DelegateCommand SaveSettingsCommand
        {
            get { return _saveSettingsCommand ??= new DelegateCommand(o => SaveSettings()); }
        }

        public string GameDirectoryLumina
        {
            get { return _settings.GameDirectoryLumina; }
            set
            {
                value = Path.Combine(value);
                if (IsValidLuminaPath(value) && _settings.GameDirectoryLumina != value)
                {
                    //settings.IsLuminaGamePathSet = true;

                    // TODO: Message box?
                    //_messageBox.Show("Please wait while Lumina is instantiated.");

                    _settings.GameDirectoryLumina = value;
                    OnPropertyChanged();
                }
            }
        }

        public string BrowseDirectory
        {
            get { return _settings.BrowseDirectory; }
            set {
                if (Directory.Exists(value))
                {
                    value = Path.Combine(value);
                    _settings.BrowseDirectory = value;
                    OnPropertyChanged();
                }
            }
        }

        public string OutputDirectory
        {
            get { return _settings.OutputDirectory; }
            set { 
                if (Directory.Exists(value))
                {
                    value = Path.Combine(value);
                    _settings.OutputDirectory = value;
                    OnPropertyChanged();
                }
            }
        }

        private string? OpenFolderBrowserDialog()
        {
            var dlg = new FolderBrowserDialog { };
            var result = dlg.ShowDialog();
            if (result == DialogResult.Cancel)
            {
                return null;
            }
            return dlg.SelectedPath;
        }

        private void SetDirectory(object o)
        {
            var str = o as string;
            var path = OpenFolderBrowserDialog();
            if (path == null)
            {
                return;
            }

            if (str == "Output")
            {
                OutputDirectory = path;
            }
            else if (str == "Browse")
            {
                BrowseDirectory = path;
            }
        }

        private void SetGamePath(object o)
        {
            var path = OpenFolderBrowserDialog();
            if (path == null)
            {
                return;
            }
            var str = o as string;
            if (str == "Lumina")
            {
                if (IsValidLuminaPath(path))
                {
                    GameDirectoryLumina = path;
                }
            }
            else if (str == "TexTools")
            {
                (bool valid, bool append) = IsValidTexToolsPath(path);
                /*
                if (valid)
                {
                    if (append)
                    {
                        GameDirectoryTexTools = Path.Combine(GameDirectoryTexTools, "ffxiv");
                    }
                    else
                    {
                        GameDirectoryTexTools = path;
                    }
                }
                */
            }
        }

        private bool IsValidLuminaPath(string path)
        {
            var separator = Path.DirectorySeparatorChar;
            if (string.IsNullOrEmpty(path))
            {
                _messageBox.Show($"Please find the directory {separator}game{separator}sqpack");
                return false;
            }
            var dir = new DirectoryInfo(path);

            if (!dir.Exists)
            {
                _messageBox.Show("Directory does not exist.");
                return false;
            }
            if (dir.Name != "sqpack")
            {
                _messageBox.Show($"The directory needs to point to {separator}sqpack.");
                return false;
            }

            return true;
        }

        private static (bool, bool append) IsValidTexToolsPath(string path)
        {
            var dataPath = new DirectoryInfo(path);

            if (string.IsNullOrEmpty(path))
            {
                MessageBox.Show("Please find the directory /game/sqpack/ffxiv");
                return (false, false);
            }

            if (dataPath.Name == "sqpack")
            {
                MessageBox.Show("Setting to /ffxiv");
                return (true, true);
            }
            if (dataPath.Name != "ffxiv")
            {
                MessageBox.Show("Could not find folder /ffxiv");
                return (false, false);
            }
            else
            {
                return (true, false);
            }
        }

        public void SaveSettings()
        {
            _settings.SaveSettings();
            //MessageBox.Show("Settings saved.");
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            SaveSettings();
        }
    }
}
