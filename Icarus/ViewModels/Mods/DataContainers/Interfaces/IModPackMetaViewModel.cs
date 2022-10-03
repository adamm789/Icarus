using Icarus.Mods.DataContainers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.ViewModels.Mods.DataContainers.Interfaces
{
    public interface IModPackMetaViewModel : INotifyPropertyChanged
    {
        public ModPack ModPack { get; }
    }
}
