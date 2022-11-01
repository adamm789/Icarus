using Icarus.Mods.DataContainers;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Icarus.ViewModels.Mods.DataContainers.Interfaces
{
    public interface IModPackViewModel : INotifyPropertyChanged
    {
        ModPack ModPack { get; }
        IModPackMetaViewModel ModPackMetaViewModel { get; }
        IModsListViewModel ModsListViewModel { get; }
        ObservableCollection<ModPackPageViewModel> ModPackPages { get; }
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
