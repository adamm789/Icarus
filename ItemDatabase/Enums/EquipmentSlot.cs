namespace ItemDatabase.Enums
{
    [Flags]
    public enum EquipmentSlot
    {
        None = 0,
        MainHand = 0x1,
        OffHand = 0x2,

        Head = 0x4,
        Body = 0x8,
        Hands = 0x16,
        Waist = 0x20,
        Legs = 0x40,
        Feet = 0x80,

        Ears = 0x100,
        Neck = 0x200,
        Wrists = 0x400,
        RightRing = 0x1000,
        LeftRing = 0x2000,

        SoulCrystal,

        Equipment = Head | Body | Hands | Legs | Feet,
        Accessory = Ears | Neck | Wrists | RightRing | LeftRing,
        Weapon = MainHand | OffHand,
        Ring = RightRing | LeftRing
    }
}