using Icarus.ViewModels.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using xivModdingFramework.Mods.DataContainers;

namespace Icarus.Old
{
    public class ModPack : NotifyPropertyChanged
    {
        public ModPack()
        {
            Name = "My Mod";
            Url = "google.com";
            Author = "User";
            Version = "1.0.0";
        }

        private ModPackJson ModPackJson = new();

        public string MinimumFrameworkVersion = "1.0.0.0";
        public string TTMPVersion
        {
            get { return ModPackJson.TTMPVersion; }
            set { ModPackJson.TTMPVersion = value; }
        }
        public string Name
        {
            get { return ModPackJson.Name; }
            set { ModPackJson.Name = value; OnPropertyChanged(); }
        }
        public string Author
        {
            get { return ModPackJson.Author; }
            set { ModPackJson.Author = value; OnPropertyChanged(); }
        }

        private Regex VersionRegex = new(@"^[0-9].[0-9].[0-9]$");

        public string Version
        {
            get { return ModPackJson.Version; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return;
                }
                if (VersionRegex.IsMatch(value))
                {
                    ModPackJson.Version = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Description
        {
            get { return ModPackJson.Description; }
            set { ModPackJson.Description = value; OnPropertyChanged(); }
        }
        public string Url
        {
            get { return ModPackJson.Url; }
            set { ModPackJson.Url = value; OnPropertyChanged(); }
        }

        List<ModPackPage> _modPackPages = new();
        List<Mod> _simpleModsList = new();

        public List<ModPackPage> ModPackPages = new();
        public List<Mod> SimpleModsList = new();

        ObservableCollection<ModPackPage> _observableModPackPages = new();
        public ObservableCollection<ModPackPage> ObservableModPackPages
        {
            get { return _observableModPackPages; }
            set { _observableModPackPages = value; OnPropertyChanged(); }
        }

        ObservableCollection<Mod> _observableSimpleModsList = new();
        public ObservableCollection<Mod> ObservableSimpleModsList
        {
            get { return _observableSimpleModsList; }
            set { _observableSimpleModsList = value; OnPropertyChanged(); }
        }

        public List<Mod> GetSimpleModsList()
        {
            return _simpleModsList;
        }
        public void Add(Mod mod)
        {
            _simpleModsList.Add(mod);
            ObservableSimpleModsList.Add(mod);
        }
        public void Remove(Mod mod)
        {
            _simpleModsList.Remove(mod);
            ObservableSimpleModsList.Remove(mod);
        }

        public List<ModPackPage> GetModPackPages()
        {
            return _modPackPages;
        }
        public void Add(ModPackPage page)
        {
            _modPackPages.Add(page);
            ObservableModPackPages.Add(page);
        }
        public void Remove(ModPackPage page)
        {
            _modPackPages.Remove(page);
            ObservableModPackPages.Remove(page);
        }
    }
}
