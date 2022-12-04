using Icarus.Mods.Interfaces;
using Icarus.ViewModels.Util;
using System.ComponentModel;

namespace Icarus.ViewModels.Mods.DataContainers
{
    public class ModOptionModViewModel : NotifyPropertyChanged
    {
        public ModViewModel Mod;
        ModOptionViewModel _parent;

        public ModOptionModViewModel(ModOptionViewModel parent, ModViewModel mod)
        {
            Mod = mod;
            _parent = parent;

            RemoveCommand = new DelegateCommand(o => _parent.RemoveMod(this));
            mod.PropertyChanged += new(OnModPropertyChanged);
        }

        // TODO: If an option is selected and being displayed on the advanced modpack part then edited, the information is not updated in the modoptionmod
        private void OnModPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ModViewModel.DestinationName)) OnPropertyChanged(nameof(DestinationName));
            if (e.PropertyName == nameof(ModViewModel.DestinationPath)) OnPropertyChanged(nameof(DestinationPath));
        }

        public IMod GetMod() => Mod.GetMod();
        public string Identifier => Mod.Identifier;
        public string FileName => Mod.FileName;
        public string DestinationName => Mod.DestinationName;
        public string DestinationPath => Mod.DestinationPath;
        public string DisplayedHeader => Mod.DisplayedHeader;
        public DelegateCommand RemoveCommand { get; }
    }
}
