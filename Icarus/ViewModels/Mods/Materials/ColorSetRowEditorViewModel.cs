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
    public class ColorSetRowEditorViewModel : NotifyPropertyChanged
    {
        List<Half> _colorSetData;
        BitArray arr = new BitArray(16);
        ColorSetRowViewModel _parent;
        MaterialMod _materialMod;
        ushort _dyeInfo;
        int _rowNumber;

        public ColorSetRowEditorViewModel(ColorSetRowViewModel parent, MaterialMod material, StainingTemplateFile dyeTemplateFile)
        {
            _parent = parent;
            _rowNumber = _parent.RowNumber;
            DisplayedRowNumber = _rowNumber + 1;
            _materialMod = material;
            _colorSetData = _materialMod.ColorSetData;

            if (_materialMod.ColorSetDyeData == null)
            {
                _materialMod.ColorSetDyeData = new byte[32];
            }

            _dyeInfo = BitConverter.ToUInt16(_materialMod.ColorSetDyeData, _rowNumber * 2);

            var flags = (_dyeInfo & 0x1F);

            UseDiffuse = (flags & 0x01) > 0;
            UseSpecular = (flags & 0x02) > 0;
            UseEmissive = (flags & 0x04) > 0;
            UseGloss = (flags & 0x08) > 0;
            UseSpecPower = (flags & 0x10) > 0;

            //var template = dyeTemplateFile.GetTemplate(_dyeTemplateId);

            foreach (var t in dyeTemplateFile.GetKeys())
            {
                Templates.Add(Convert.ToString(t));
            }

            Templates.Insert(0, "None");
            var val = (ushort)(_dyeInfo >> 5);

            if (val == 0)
            {
                DyeTemplateId = "None";
            }
            else
            {
                DyeTemplateId = Convert.ToString(val);
            }

            SpecularPower = _colorSetData[(_rowNumber * 16) + 3];
            GlossBox = _colorSetData[(_rowNumber * 16) + 7];
            TileId = (int)(_colorSetData[(_rowNumber * 16) + 11] * 64);

            TileCountX = _colorSetData[(_rowNumber * 16) + 12];
            TileSkewX = _colorSetData[(_rowNumber * 16) + 13];
            TileSkewY = _colorSetData[(_rowNumber * 16) + 14];
            TileCountY = _colorSetData[(_rowNumber * 16) + 15];
        }

        #region Bindings
        public int DisplayedRowNumber { get; }
        public ColorViewModel DiffuseColor => _parent.DiffuseColor;
        public ColorViewModel SpecularColor => _parent.SpecularColor;
        public ColorViewModel EmissiveColor => _parent.EmissiveColor;

        List<string> _templates = new();
        public List<string> Templates
        {
            get { return _templates; }
            set { _templates = value; OnPropertyChanged(); }
        }

        bool _canEditDye = false;
        public bool CanEditDye
        {
            get { return _canEditDye; }
            set { _canEditDye = value; OnPropertyChanged(); }
        }

        bool _canApplyToEmissive = false;
        public bool CanApplyToEmissive
        {
            get { return _canApplyToEmissive; }
            set { _canApplyToEmissive = value; OnPropertyChanged(); }
        }

        // TODO: IF a row is dyeable, distinguish it from non-dyeable rows
        string _dyeTemplateId;
        public string DyeTemplateId
        {
            get { return _dyeTemplateId; }
            set
            {
                _dyeTemplateId = value;
                OnPropertyChanged();
                BitArray b;

                if (value == "None")
                {
                    b = new BitArray(BitConverter.GetBytes((ushort)0));
                    CanEditDye = false;
                    savedDyes = new() { UseDiffuse, UseSpecular, UseEmissive, UseGloss, UseSpecPower };

                    UseDiffuse = false;
                    UseSpecular = false;
                    UseEmissive = false;
                    UseGloss = false;
                    UseSpecPower = false;
                }
                else
                {
                    var val = Convert.ToUInt16(value);
                    b = new BitArray(BitConverter.GetBytes((ushort)(val << 5)));
                    CanEditDye = true;

                    // I guess?
                    if (val >= 510 && val <= 522)
                    {
                        CanApplyToEmissive = true;
                    }
                    else
                    {
                        CanApplyToEmissive = false;
                    }

                    if (savedDyes != null && savedDyes.Count > 0)
                    {
                        UseDiffuse = savedDyes[0];
                        UseSpecular = savedDyes[1];
                        UseEmissive = savedDyes[2];
                        UseGloss = savedDyes[3];
                        UseSpecPower = savedDyes[4];
                        savedDyes = null;
                    }

                }
                if (b.Length == 16)
                {
                    arr.Or(b);
                    arr.CopyTo(_materialMod.ColorSetDyeData, _rowNumber * 2);
                }
                //_materialMod.ColorSetDyeData[_rowNumber * 2] = (byte)value;
            }
        }

        List<bool>? savedDyes;

        bool _useDiffuse = false;
        public bool UseDiffuse
        {
            get { return _useDiffuse; }
            set
            {
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
            set
            {
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
            set
            {
                _useEmissive = value;
                OnPropertyChanged();
                arr[2] = value;
                arr.CopyTo(_materialMod.ColorSetDyeData, _rowNumber * 2);
            }
        }

        bool _useGloss = false;
        public bool UseGloss
        {
            get { return _useGloss; }
            set
            {
                _useGloss = value;
                OnPropertyChanged();
                arr[3] = value;
                arr.CopyTo(_materialMod.ColorSetDyeData, _rowNumber * 2);
            }
        }

        bool _useSpecPower = false;
        public bool UseSpecPower
        {
            get { return _useSpecPower; }
            set
            {
                _useSpecPower = value;
                OnPropertyChanged();
                arr[4] = value;
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
        int _tileId;
        public int TileId
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
        #endregion
    }
}
