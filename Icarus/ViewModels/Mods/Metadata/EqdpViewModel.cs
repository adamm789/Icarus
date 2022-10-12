using Icarus.Mods;
using Icarus.ViewModels.Util;
using ItemDatabase.Paths;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.General.Enums;
using xivModdingFramework.Models.DataContainers;

namespace Icarus.ViewModels.Mods.Metadata
{
    public class EqdpViewModel : NotifyPropertyChanged
    {        public EqdpViewModel(Dictionary<XivRace, EquipmentDeformationParameter> dict)
        {
            foreach (var race in XivRaces.PlayableRaces)
            {
                dict.TryGetValue(race, out var eqdp);
                var vm = new EqdpEntryViewModel(race, eqdp);
                if (XivPathParser.IsMaleSkin(race))
                {
                    MaleEqdpEntries.Add(vm);
                }
                else
                {
                    FemaleEqdpEntries.Add(vm);
                }
            }
        }

        public Dictionary<XivRace, EquipmentDeformationParameter> GetEntries()
        {
            var ret = new Dictionary<XivRace, EquipmentDeformationParameter>();
            foreach (var vm in MaleEqdpEntries)
            {
                ret.Add(vm.Race, vm.Parameter);
            }
            foreach (var vm in FemaleEqdpEntries)
            {
                ret.Add(vm.Race, vm.Parameter);
            }
            return ret;
        }

        ObservableCollection<EqdpEntryViewModel> _maleEqdpEntries = new();
        public ObservableCollection<EqdpEntryViewModel> MaleEqdpEntries
        {
            get { return _maleEqdpEntries; }
            set { _maleEqdpEntries = value; OnPropertyChanged(); }
        }

        ObservableCollection<EqdpEntryViewModel> _femaleEqdpEntries = new();
        public ObservableCollection<EqdpEntryViewModel> FemaleEqdpEntries
        {
            get { return _femaleEqdpEntries; }
            set { _femaleEqdpEntries = value; OnPropertyChanged(); }
        }
    }
}
