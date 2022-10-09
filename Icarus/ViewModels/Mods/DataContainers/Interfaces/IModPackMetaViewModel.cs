using Icarus.Mods.DataContainers;
using System.ComponentModel;

namespace Icarus.ViewModels.Mods.DataContainers.Interfaces
{
    public interface IModPackMetaViewModel : INotifyPropertyChanged
    {
        public ModPack ModPack { get; }
    }
}
