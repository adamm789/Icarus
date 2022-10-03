using Icarus.ViewModels.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using xivModdingFramework.Mods.DataContainers;

namespace Icarus.Old
{
    public class ModOption : NotifyPropertyChanged
    {
        private ModOptionJson ModOptionJson = new();

        public string Name
        {
            get { return ModOptionJson.Name; }
            set { ModOptionJson.Name = value; OnPropertyChanged();  }
        }

        public string Description
        {
            get { return ModOptionJson.Description; }
            set { ModOptionJson.Description = value; OnPropertyChanged(); }
        }
        public string ImagePath
        {
            get { return ModOptionJson.ImagePath; }
            set { ModOptionJson.ImagePath = value; }
        }
        private List<ModsJson> ModsJson
        {
            get { return ModOptionJson.ModsJsons; }
            set { ModOptionJson.ModsJsons = value; }
        }
        public string GroupName
        {
            get { return ModOptionJson.GroupName; }
            set { ModOptionJson.GroupName = value; }
        }
        public string SelectionType
        {
            get { return ModOptionJson.SelectionType; }
            set { ModOptionJson.SelectionType = value; OnPropertyChanged(); }
        }
        public bool IsChecked
        {
            get { return ModOptionJson.IsChecked; }
            set { ModOptionJson.IsChecked = value; }
        }

        public List<Mod> Mods = new();

        ObservableCollection<Mod> _modsList = new();
        public ObservableCollection<Mod> ModsList
        {
            get { return _modsList; }
            set { _modsList = value; OnPropertyChanged(); }
        }

        public void Add(Mod mod)
        {
            ModsList.Add(mod);
        }
        public void Remove(Mod mod)
        {
            ModsList.Remove(mod);
        }

        public List<Mod> GetMods()
        {
            return Mods;
        }
    }
}
