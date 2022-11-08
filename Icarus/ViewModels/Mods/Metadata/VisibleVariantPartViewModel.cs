using Icarus.ViewModels.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.SqPack.FileTypes;
using xivModdingFramework.Variants.DataContainers;

namespace Icarus.ViewModels.Mods.Metadata
{
    public class VisibleVariantPartViewModel : NotifyPropertyChanged
    {

        int _index;
        ImcEntryViewModel _imc;

        BitArray arr = new BitArray(16);

        public VisibleVariantPartViewModel(ImcEntryViewModel imc, int index)
        {
            _imc = imc;
            _index = index;

            PartLabel = Convert.ToChar('A' + _index);
        }

        bool _isEnabled;
        public bool IsEnabled
        {
            get { return (_imc.Mask & (1 << _index)) > 0; }
            set
            {
                // TODO: Better method to change and assign _imc.Mask?
                _isEnabled = value;
                OnPropertyChanged();
                arr = new(BitConverter.GetBytes(_imc.Mask));
                arr[_index] = value;
                var u = new int[1];
                arr.CopyTo(u, 0);
                _imc.Mask = (ushort)u[0];
            }
        }

        char _partLabel;
        public char PartLabel
        {
            get { return _partLabel; }
            set { _partLabel = value; OnPropertyChanged(); }
        }
    }
}
