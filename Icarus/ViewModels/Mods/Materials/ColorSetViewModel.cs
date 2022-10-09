using Icarus.ViewModels.Util;
using System.Collections.ObjectModel;

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
