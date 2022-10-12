using Icarus.ViewModels.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.Models.DataContainers;

namespace Icarus.ViewModels.Mods.Metadata
{
    public class EqpEntryViewModel : NotifyPropertyChanged
    {
        EquipmentParameterFlag _eqpFlag;
        public EquipmentParameterFlag EqpFlag
        {
            get { return _eqpFlag; }
            set { _eqpFlag = value; OnPropertyChanged(); }
        }

        bool _eqpBool;
        public bool EqpBool
        {
            get { return _eqpBool; }
            set { _eqpBool = value; OnPropertyChanged(); }
        }

        public EqpEntryViewModel(EquipmentParameterFlag flag, bool b)
        {
            EqpFlag = flag;
            EqpBool = b;
        }
    }
}
