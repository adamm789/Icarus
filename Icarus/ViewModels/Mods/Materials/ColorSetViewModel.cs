using Icarus.ViewModels.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.ViewModels.Mods.Materials
{
    public class ColorSetViewModel : NotifyPropertyChanged
    {
        ObservableCollection<ColorSetRowViewModel> _colorSetRows;
        public ObservableCollection<ColorSetRowViewModel> ColorSetRows
        {
            get { return _colorSetRows; }
            set { _colorSetRows = value; OnPropertyChanged(); }
        }
    }
}
