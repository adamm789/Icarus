using Icarus.Mods;
using Icarus.ViewModels.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.Materials.DataContainers;
using xivModdingFramework.Materials.FileTypes;
using Half = SharpDX.Half;


namespace Icarus.ViewModels.Mods.Materials
{
    public class ColorSetRowDyeDataViewModel : NotifyPropertyChanged
    {
        List<Half> _colorSetData;
        ushort _dyeTemplateId;
        BitArray arr = new BitArray(16);

        MaterialMod _materialMod;

        ushort _dyeInfo;
        int _rowNumber;
        public ColorSetRowDyeDataViewModel(int rowNumber, MaterialMod material, StainingTemplateFile dyeTemplateFile)
        {
            _rowNumber = rowNumber;
            _materialMod = material;
            _colorSetData = _materialMod.ColorSetData;

            if (_materialMod.ColorSetDyeData == null)
            {
                _materialMod.ColorSetDyeData = new byte[32];
            }

            _dyeInfo = BitConverter.ToUInt16(_materialMod.ColorSetDyeData, rowNumber);

            var flags = (_dyeInfo & 0x1F);
            UseDiffuse = (flags & 0x01) > 0;
            UseSpecular = (flags & 0x02) > 0;
            UseEmissive = (flags & 0x04) > 0;
            UseGloss = (flags & 0x08) > 0;
            UseSpecPower = (flags & 0x10) > 0;

            _dyeTemplateId = (ushort)(_dyeInfo >> 5);
            var template = dyeTemplateFile.GetTemplate(_dyeTemplateId);

            SpecularPower = _colorSetData[(_rowNumber * 16) + 3];
            GlossBox = _colorSetData[(_rowNumber * 16) + 7];
            TileId = _colorSetData[(_rowNumber * 16) + 11] * 64;

            TileCountX = _colorSetData[(_rowNumber * 16) + 12];
            TileSkewX = _colorSetData[(_rowNumber * 16) + 13];
            TileSkewY = _colorSetData[(_rowNumber * 16) + 14];
            TileCountY = _colorSetData[(_rowNumber * 16) + 15];
            
        }

        bool _useDiffuse = false;
        public bool UseDiffuse
        {
            get { return _useDiffuse; }
            set {
                _useDiffuse = value;
                OnPropertyChanged();
                arr[0] = value;
                arr.CopyTo(_materialMod.ColorSetDyeData, _rowNumber * 2);
            }
        }

        bool _useSpecular = false;
        public bool UseSpecular
        {
            get { return _useSpecular; }
            set {
                _useSpecular = value;
                OnPropertyChanged();
                arr[1] = value;
                arr.CopyTo(_materialMod.ColorSetDyeData, _rowNumber * 2);
            }
        }

        bool _useEmissive = false;
        public bool UseEmissive
        {
            get { return _useEmissive; }
            set {
                _useEmissive = value;
                OnPropertyChanged();
                arr[3] = value;
                arr.CopyTo(_materialMod.ColorSetDyeData, _rowNumber * 2);
            }
        }

        bool _useGloss = false;
        public bool UseGloss
        {
            get { return _useGloss; }
            set {
                _useGloss = value; 
                OnPropertyChanged();
                arr[7] = value;
                arr.CopyTo(_materialMod.ColorSetDyeData, _rowNumber * 2);
            }
        }

        bool _useSpecPower = false;
        public bool UseSpecPower
        {
            get { return _useSpecPower; }
            set {
                _useSpecPower = value;
                OnPropertyChanged();
                arr[9] = value;
                arr.CopyTo(_materialMod.ColorSetDyeData, _rowNumber * 2);
            }
        }

        public float SpecularPower
        {
            get { return _colorSetData[(_rowNumber * 16) + 3]; }
            set { _colorSetData[(_rowNumber * 16) + 3] = value; OnPropertyChanged(); }
        }
        public float GlossBox
        {
            get { return _colorSetData[(_rowNumber * 16) + 7]; }
            set { _colorSetData[(_rowNumber * 16) + 7] = value; OnPropertyChanged(); }
        }
        float _tileId;
        public float TileId
        {
            get { return _tileId; }
            set
            {
                _tileId = value;
                OnPropertyChanged();
                // TODO: Tile ID?
                _colorSetData[(_rowNumber * 16) + 11] = value / 64;
            }
        }

        public float TileCountX
        {
            get { return _colorSetData[(_rowNumber * 16) + 12]; }
            set { _colorSetData[(_rowNumber * 16) + 12] = value; OnPropertyChanged(); }
        }
        public float TileSkewX
        {
            get { return _colorSetData[(_rowNumber * 16) + 13]; }
            set { _colorSetData[(_rowNumber * 16) + 13] = value; OnPropertyChanged(); }
        }
        public float TileSkewY
        {
            get { return _colorSetData[(_rowNumber * 16) + 14]; }
            set { _colorSetData[(_rowNumber * 16) + 14] = value; OnPropertyChanged(); }
        }
        public float TileCountY
        {
            get { return _colorSetData[(_rowNumber * 16) + 15]; }
            set { _colorSetData[(_rowNumber * 16) + 15] = value; OnPropertyChanged(); }
        }
    }
}
