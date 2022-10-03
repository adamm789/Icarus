using Icarus.Mods.DataContainers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.ViewModels.Mods.DataContainers.Interfaces
{
    public interface IModPackViewModel : INotifyPropertyChanged
    {
        ModPack ModPack { get; }
        ObservableCollection<ModPackPageViewModel> ModPackPages { get; set; }
        ModPackPageViewModel AddPage();
        void AddPage(ModPackPageViewModel packPage);
        void RemovePage(ModPackPageViewModel packPage);
        void Move(ModPackPageViewModel source, ModPackPageViewModel target);
        bool ArePagesEmpty();
        void SetModPack(ModPack pack);
        void SetModPack(ModPack pack, ModPackViewModelImportFlags flags);
        void IncreasePageIndex();
    }
}
