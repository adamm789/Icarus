using Icarus.ViewModels.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.Models.DataContainers;

namespace Icarus.ViewModels.Mods.Metadata
{
    public class GmpViewModel : NotifyPropertyChanged
    {
        public GimmickParameter GimmickParameter { get; }

        public GmpViewModel(GimmickParameter gimmickParameter)
        {
            GimmickParameter = gimmickParameter;
        }

        public List<string> AnimatedOptions { get; } = new()
        {
            "Instant", "Animated"
        };

        public bool Enabled
        {
            get { return GimmickParameter.Enabled; }
            set { GimmickParameter.Enabled = value; OnPropertyChanged(); }
        }
        public bool Animated
        {
            get { return GimmickParameter.Animated; }
            set { GimmickParameter.Animated = value; OnPropertyChanged(); }
        }

        int _animatedIndex = 0;
        public int AnimatedIndex
        {
            get { return _animatedIndex; }
            set
            {
                _animatedIndex = value;
                OnPropertyChanged();
                Animated = AnimatedIndex > 0;
            }
        }
        public ushort RotationA
        {
            get { return GimmickParameter.RotationA; }
            set
            {
                if (IsValidRotation(value))
                {
                    GimmickParameter.RotationA = value;
                    OnPropertyChanged();
                }
            }
        }
        public ushort RotationB
        {
            get { return GimmickParameter.RotationB; }
            set
            {
                if (IsValidRotation(value))
                {
                    GimmickParameter.RotationB = value;
                    OnPropertyChanged();
                }
            }
        }
        public ushort RotationC
        {
            get { return GimmickParameter.RotationC; }
            set
            {
                if (IsValidRotation(value))
                {
                    GimmickParameter.RotationC = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool IsValidRotation(ushort value)
        {
            return value >= 0 && value <= 1023;
        }
        public byte UnknownHigh
        {
            get { return GimmickParameter.UnknownHigh; }
            set {
                if (IsValidUnknownBytes(value))
                {
                    GimmickParameter.UnknownHigh = value;
                    OnPropertyChanged();
                }
            }
        }
        public byte UnknownLow
        {
            get { return GimmickParameter.UnknownLow; }
            set {
                if (IsValidUnknownBytes(value))
                {
                    GimmickParameter.UnknownLow = value;
                    OnPropertyChanged();
                }
            }
        }
        private bool IsValidUnknownBytes(byte value)
        {
            return value >= 0 && value < 16;
        }
    }
}
