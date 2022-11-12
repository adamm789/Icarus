using Icarus.ViewModels.Util;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Half = SharpDX.Half;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;
using Color = System.Windows.Media.Color;
using System.Windows.Data;
using Icarus.Mods;

namespace Icarus.ViewModels.Mods.Materials
{
    public class ColorViewModel : NotifyPropertyChanged
    {
        MaterialMod _material;
        int _offset;

        public ColorViewModel(MaterialMod material, int offset)
        {
            _material = material;
            _offset = offset;

            OriginalR = _material.ColorSetData[offset];
            OriginalG = _material.ColorSetData[offset + 1];
            OriginalB = _material.ColorSetData[offset + 2];
        }

        public void Copy(ColorViewModel color)
        {
            R = color.R;
            G = color.G;
            B = color.B;
        }

        public void ResetValues()
        {
            R = OriginalR;
            G = OriginalG;
            B = OriginalB;
        }

        private bool IsValidValue(float value)
        {
            return value >= 0 && value <= 1;
        }

        DelegateCommand _resetCommand;
        public DelegateCommand ResetCommand
        {
            get { return _resetCommand ??= new DelegateCommand(o => ResetValues()); }
        }

        // TODO: Better color editor than three textboxes
        public float OriginalR { get; }
        public float OriginalG { get; }
        public float OriginalB { get; }

        public float R
        {
            get {
                return _material.ColorSetData[_offset];
                }
            set
            {
                if (!IsValidValue(value)) return;
                _material.ColorSetData[_offset] = (Half)value;
                OnPropertyChanged(); OnPropertyChanged(nameof(BrushColor));
            }
        }

        public float G
        {
            get
            {
                return _material.ColorSetData[_offset + 1];
            }
            set
            {
                if (!IsValidValue(value)) return;
                _material.ColorSetData[_offset + 1] = (Half)value;
                OnPropertyChanged(); OnPropertyChanged(nameof(BrushColor));

            }
        }
        public float B
        {
            get
            {
                return _material.ColorSetData[_offset + 2];
            }
            set
            {
                if (!IsValidValue(value)) return;
                _material.ColorSetData[_offset + 2] = (Half)value;
                OnPropertyChanged(); OnPropertyChanged(nameof(BrushColor));
            }
        }

        SolidColorBrush _brushColor = new();
        public SolidColorBrush BrushColor
        {
            get
            {
                var color = Color.FromArgb(255, (byte)(R * 255), (byte)(G * 255), (byte)(B * 255));
                _brushColor.Color = color;
                return _brushColor;
            }
        }
    }
}
