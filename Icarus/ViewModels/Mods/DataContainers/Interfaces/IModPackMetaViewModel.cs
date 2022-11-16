using Icarus.Mods.DataContainers;
using System.ComponentModel;

namespace Icarus.ViewModels.Mods.DataContainers.Interfaces
{
    public interface IModPackMetaViewModel : INotifyPropertyChanged
    {
        public ModPack ModPack { get; }
        public string Name { get; set; }
        public void CopyFrom(ModPack modPack);
    }
}
