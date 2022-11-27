using System.ComponentModel;

namespace ItemDatabase.Enums
{
    // https://docs.google.com/spreadsheets/d/1kIKvVsW3fOnVeTi9iZlBDqJo6GWVn6K6BCUIRldEjhw/edit#gid=1898269590
    public enum XivAttribute
    {
        Null,

        [Description("atr_mv_{variant}")]
        HeadVariantParts,
        [Description("atr_inr")]
        Gorget,

        // Body
        [Description("atr_tv_{variant}")]
        BodyVariantParts,
        [Description("atr_hij")]
        Wrist,
        [Description("atr_nek")]
        Neck,
        [Description("atr_ude")]
        Elbow,

        // Gloves
        [Description("atr_gv_{variant}")]
        GloveVariantParts,
        [Description("atr_arm")]
        Glove,

        // Legs
        [Description("atr_dv_{variant}")]
        LegVariantParts,
        [Description("atr_hiz")]
        Knee,
        [Description("atr_kod")]
        Waist,
        [Description("atr_sne")]
        Shin,

        // Shoes
        [Description("atr_sv_{variant}")]
        ShoeVariantParts,
        [Description("atr_leg")]
        Boot,
        [Description("atr_lpd")]
        KneePad,

        // Accessories

        [Description("atr_ev_{variant}")]
        EarringVariantParts,
        [Description("atr_nv_{variant}")]
        NecklaceVariantParts,
        [Description("atr_wv_{variant}")]
        BraceletVariantParts,
        [Description("atr_rv_{variant}")]
        RingVariantParts,
        

        // Weapons
        /*
        OtherWeaponVariantParts,
        WeaponSpecificParts,
        */
        Arrow,
        ArrowQuiver,
        GaussBarrel,

        // Common/Misc
        [Description("atr_lod")]
        ExcessDetail,
        [Description("atr_tlh")]
        NonTailRaces,
        [Description("atr_tls")]
        TailRaces,
        [Description("atr_top")]
        MiqoteEars,

        // Unusual/Strange
        [Description("atr_hair")]
        AuraFemaleFaces,
        [Description("atr_sta")]
        MiqoteHair,

        // Face
        [Description("atr_fv_{variant}")]
        FaceVariantParts,
        [Description("atr_hig")]
        FacialHair,
        [Description("atr_hrn")]
        Horns,
        [Description("atr_kao")]
        Face,
        [Description("atr_mim")]
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
