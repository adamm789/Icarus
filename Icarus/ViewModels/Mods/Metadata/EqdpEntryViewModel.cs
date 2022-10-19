using Icarus.Mods;
using Icarus.ViewModels.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.General.Enums;
using xivModdingFramework.Models.DataContainers;

namespace Icarus.ViewModels.Mods.Metadata
{
    public class EqdpEntryViewModel : NotifyPropertyChanged
    {
        public EquipmentDeformationParameter Parameter { get; }
        public XivRace Race { get; }

        public bool Bit0
        {
            get { return Parameter.bit0; }
            set { Parameter.bit0 = value; OnPropertyChanged(); }
        }
        public bool Bit1
        {
            get { return Parameter.bit1; }
            set { Parameter.bit1 = value; OnPropertyChanged(); }
        }

        public EqdpEntryViewModel(XivRace race, EquipmentDeformationParameter parameter)
        {
            Race = race;
            Parameter = parameter;
        }
    }
}
