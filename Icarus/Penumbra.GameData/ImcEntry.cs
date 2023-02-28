using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.Penumbra.GameData
{
    //https://github.com/xivdev/Penumbra/blob/master/Penumbra.GameData/Structs/ImcEntry.cs
    public class ImcEntry
    {
        public byte MaterialId { get; init; }
        public byte DecalId { get; init; }
        public readonly ushort AttributeAndSound;
        public byte VfxId { get; init; }
        public byte MaterialAnimationId { get; init; }

        public ushort AttributeMask
        {
            get => (ushort)(AttributeAndSound & 0x3FF);
            init => AttributeAndSound = (ushort)((AttributeAndSound & ~0x3FF) | (value & 0x3FF));
        }

        public byte SoundId
        {
            get => (byte)(AttributeAndSound >> 10);
            init => AttributeAndSound = (ushort)(AttributeMask | (value << 10));
        }
    }
}
