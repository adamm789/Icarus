using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.Mods.Penumbra
{
    public class PenumbraMetadataEntry
    {
        //public PenumbraImcEntry Entry = new();

        public int AttributeAndSound = 0;
        public int MaterialId = 0;
        public int DecalId = 0;
        public int VfxId = 0;
        public int MaterialAnimationId = 0;
        public int AttributeMask = 0;
        public int SoundId = 0;

        public float PrimaryId = 0;
        public int SecondaryId = 0;
        public int Variant = 0;
        public string ObjectType = "";
        public string EquipSlot = "";
        public string BodySlot = "";
        public bool Valid = true;
    }
}
