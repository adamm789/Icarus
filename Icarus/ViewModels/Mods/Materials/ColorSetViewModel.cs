using Icarus.ViewModels.Util;
using SharpDX;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Documents;

namespace Icarus.ViewModels.Mods.Materials
{
    public class ColorSetViewModel : NotifyPropertyChanged
    {
        public ColorSetViewModel(List<Half> colorset)
        {
            if (colorset.Count == 256)
            {
                for (var i = 0; i < 16; i++)
                {
                    ColorSetRows.Add(new ColorSetRowViewModel(i, colorset.GetRange(i * 16, 16)));
                }
            }
        }

        ObservableCollection<ColorSetRowViewModel> _colorSetRows = new();
        public ObservableCollection<ColorSetRowViewModel> ColorSetRows
        {
            get { return _colorSetRows; }
            set { _colorSetRows = value; OnPropertyChanged(); }
        }
    }
}
