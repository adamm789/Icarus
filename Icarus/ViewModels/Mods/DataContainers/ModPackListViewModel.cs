using Icarus.ViewModels.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.ViewModels.Mods.DataContainers
{
    public class ModPackListViewModel : NotifyPropertyChanged
    {
        public ObservableCollection<ModPackViewModel> ModPacks { get; } = new();

        ModPackViewModel _displayedModPack;
        public ModPackViewModel DisplayedModPack
        {
            get { return _displayedModPack; }
            set { _displayedModPack = value; OnPropertyChanged(); }
        }

        public void Add(ModPackViewModel modPack)
        {

        }
    }
}
