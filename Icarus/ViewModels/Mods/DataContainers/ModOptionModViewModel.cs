using Icarus.Mods.Interfaces;
using Icarus.ViewModels.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.ViewModels.Mods.DataContainers
{
    public class ModOptionModViewModel : NotifyPropertyChanged
    {
        public ModViewModel ModViewModel;
        ModOptionViewModel _parent;

        public ModOptionModViewModel(ModOptionViewModel parent, ModViewModel mod)
        {
            ModViewModel = mod;
            _parent = parent;
            RemoveCommand = new DelegateCommand(o => _parent.RemoveMod(this));
        }

        public IMod GetMod() => ModViewModel.GetMod();
        public string Identifier => ModViewModel.Identifier;
        public string FileName => ModViewModel.FileName;
        public string DestinationName => ModViewModel.DestinationName;
        public DelegateCommand RemoveCommand { get; }
    }
}
