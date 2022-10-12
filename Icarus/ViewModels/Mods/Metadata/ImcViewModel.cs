using Icarus.ViewModels.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.Variants.DataContainers;

namespace Icarus.ViewModels.Mods.Metadata
{
    public class ImcViewModel : NotifyPropertyChanged
    {
        public XivImc XivImc { get; set; }

        public byte MaterialSet { 
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

        public byte Animation
        {
            get { return XivImc.Animation; }
            set { XivImc.Animation = value; OnPropertyChanged(); }
        }

        public ImcViewModel(XivImc imc)
        {
            XivImc = imc;
        }
    }
}
