using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemDatabase.Enums
{
    // https://docs.google.com/spreadsheets/d/1kIKvVsW3fOnVeTi9iZlBDqJo6GWVn6K6BCUIRldEjhw/edit#gid=1898269590
    public enum XivAttribute
    {
        Null,

        // Head
        Gorget,

        // Body
        //BodyVariantParts,
        Wrist,
        Neck,
        Elbow,

        // Gloves
        //GloveVariantParts,
        Glove,

        // Legs
        //LegVariantParts,
        Knee,
        Waist,
        Shin,

        // Shoes
        //ShoeVariantParts,
        Boot,
        KneePad,

        // Accessories
        /*
        EarringVariantParts,
        NecklaceVariantParts,
        BraceletVariantParts,
        RingVariantParts,
        */

        // Weapons
        /*
        OtherWeaponVariantParts,
        WeaponSpecificParts,
        */
        Arrow,
        ArrowQuiver,
        GaussBarrel,

        // Common/Misc
        ExcessDetail,
        NonTailRaces,
        TailRaces,
        MiqoteEars,

        // Unusual/Strange
        AuraFemaleFaces,
        MiqoteHair,

        // Face
        FaceVariantParts,
        FacialHair,
        Horns,
        Face,
        Ears,

        // Hair
        HairVariantParts,
        Scalp,

        // Monsters/Demihumans
        //OtherVariantParts,
        MonsterSpecificParts,

        // Extreme Rare/Unknown
        WeaponModels,
        BodyModels,

        // Connectory Attributes
        NeckConnector,
        WristConnector,
        AnkleConnector,
        WaistConnector
    }
}
