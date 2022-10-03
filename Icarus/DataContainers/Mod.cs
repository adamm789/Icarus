using Icarus.ViewModels.Util;
using xivModdingFramework.Mods.DataContainers;

namespace Icarus.Old
{
    
    public abstract class Mod : NotifyPropertyChanged
    {
        //public string Name = "";    // Name of replacement item in-game
        //public string Category = "";
        //public string DestinationPath = ""; // FullPath

        private ModsJson ModsJson = new();

        string _filePath = "";    // Path to the imported file

        /// <summary>
        /// Path to the user-imported item
        /// </summary>
        public string FilePath
        {
            get { return _filePath; }
            set
            {
                if (_filePath != value)
                {
                    _filePath = value;
                    OnPropertyChanged();
                }
            }
        }

        public string DestinationName
        {
            get { return ModsJson.Name; }
            set { 
                ModsJson.Name = value;
                OnPropertyChanged();
            }
        }
        public string DestinationPath
        {
            get { return ModsJson.FullPath; }
            set { 
                if (ModsJson.FullPath != value)
                {
                    ModsJson.FullPath = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Category
        {
            get { return ModsJson.Category; }
            set { ModsJson.Category = value; }
        }

        bool _canExport = false;
        public bool CanExport
        {
            get { return _canExport; }
            set
            {
                if (_canExport != value)
                {
                    _canExport = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
