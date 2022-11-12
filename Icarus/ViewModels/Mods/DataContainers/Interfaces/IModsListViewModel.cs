using Icarus.Mods.DataContainers;
using Icarus.Mods.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Icarus.ViewModels.Mods.DataContainers.Interfaces
{
    public interface IModsListViewModel : INotifyPropertyChanged
    {
        ModPack ModPack { get; }
        bool CanExport { get; set; }
        ObservableCollection<ModViewModel> SimpleModsList { get; }
        int AddRange(IEnumerable<IMod> mods);
        ModViewModel? Add(IMod mod);
        void AddRange(IEnumerable<ModViewModel> mods);
        void Add(ModViewModel mod);
        bool DeleteMod(ModOptionModViewModel mod);
        bool DeleteMod(ModViewModel mod);
        void Move(ModViewModel source, ModViewModel target);
        FilteredModsListViewModel FilteredModsList { get; }
    }
}
