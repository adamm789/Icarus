using Icarus.ViewModels.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.Variants.DataContainers;

namespace Icarus.ViewModels.Mods.Metadata
{
    public class ImcEntryViewModel : NotifyPropertyChanged
    {
        public XivImc XivImc { get; set; }

        // TODO: Ability to add MaterialSet...?
        public byte MaterialSet
        {
            get { return XivImc.MaterialSet; }
            set { XivImc.MaterialSet = value; OnPropertyChanged(); }
        }

        public byte Decal
        {
            get { return XivImc.Decal; }
            set { XivImc.Decal = value; OnPropertyChanged(); }
        }

        public ushort Mask
        {
            get { return XivImc.Mask; }
            set { XivImc.Mask = value; OnPropertyChanged(); }
        }

        public byte Vfx
        {
            get { return XivImc.Vfx; }
            set { XivImc.Vfx = value; OnPropertyChanged(); }
        }

        public int Sfx
        {
            get { return Mask >> 10; }
            set {

                var arr = new BitArray(BitConverter.GetBytes(XivImc.Mask));
                var val = new BitArray(BitConverter.GetBytes(value));

                // TODO: Better method to change and assign Sfx (Mask >> 10)
                arr[10] = val[0];
                arr[11] = val[1];
                arr[12] = val[2];
                arr[13] = val[3];
                arr[14] = val[4];
                arr[15] = val[5];

                var bytes = new int[1];
                arr.CopyTo(bytes, 0);
                Mask = (ushort)bytes[0];
            }
        }

        public byte Animation
        {
            get { return XivImc.Animation; }
            set { XivImc.Animation = value; OnPropertyChanged(); }
        }

        public ObservableCollection<VisibleVariantPartViewModel> Parts { get; } = new();

        public ImcEntryViewModel(XivImc imc)
        {
            XivImc = imc;
            for (var i = 0; i < 10; i++)
            {
                var p = new VisibleVariantPartViewModel(this, i);
                Parts.Add(p);
            }
        }
    }
}
