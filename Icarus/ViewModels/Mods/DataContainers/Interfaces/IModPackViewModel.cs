using Icarus.Mods.DataContainers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Icarus.ViewModels.Mods.DataContainers.Interfaces
{
    public interface IModPackViewModel : INotifyPropertyChanged
    {
        ModPack ModPack { get; }
        IModPackMetaViewModel ModPackMetaViewModel { get; }
        IModsListViewModel ModsListViewModel { get; }
        ObservableCollection<ModPackPageViewModel> ModPackPages { get; }
        ModPackPageViewModel AddPage();

        /// <summary>
        /// Adds the mods in <paramref name="pack"/>.SimpleModsList to <paramref name="ModsListViewModel"/>
        /// </summary>
        /// <param name="pack"></param>
        void Add(ModPack pack);
        void AddPage(ModPackPageViewModel packPage);
        void InsertPage(ModPackPageViewModel packPage, int index);
        void CopyPage(ModPackPageViewModel packPage);
        void RemovePage(ModPackPageViewModel packPage);
        void Move(ModPackPageViewModel source, ModPackPageViewModel target);
        bool ArePagesEmpty();

        void IncreasePageIndex();
        List<int> GetAvailablePageIndices();
        INotifyPropertyChanged DisplayedViewModel { get; set; }
        ModOptionViewModel SelectedOption { get; set; }
        void SetMetadata(ModPackMetaViewModel meta);
    }
}
