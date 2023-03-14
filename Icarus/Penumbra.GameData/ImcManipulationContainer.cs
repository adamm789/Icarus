using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.Penumbra.GameData
{
    public class ImcManipulationContainer : MetaManipulationContainer
    {
        public ImcManipulation Manipulation { get; init; }
        public ushort PrimaryId { get; init; }
        public ushort SecondaryId { get; init; }
        public byte Variant { get; init; }
        public string ObjectType;
        public string EquipSlot;
        public string BodySlot;
        public bool Valid;
    }
}
