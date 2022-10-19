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
    {
        public EqdpViewModel(Dictionary<XivRace, EquipmentDeformationParameter> dict)
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

        public ObservableCollection<EqdpEntryViewModel> MaleEqdpEntries { get; } = new();
        public ObservableCollection<EqdpEntryViewModel> FemaleEqdpEntries { get; } = new();
    }
}
