using ItemDatabase.Enums;
using xivModdingFramework.General.Enums;


namespace ItemDatabase
{
    // TODO: Better name
    public class Chara : Gear
    {
        public Chara(EquipmentSlot slot)
        {
            Slot = slot;
            Category = MainItemCategory.Gear;

            Name = _smallClothesDict[Slot];
            _directory = "chara/equipment/";
            _baseString = "e0000";
            //_slotName = _suffixDict[Slot];
            _slotName = Slot.GetShortHandSlot(true);

            _shortVariantString = "v00";
            _longVariantString = "v0000";
            MaterialId = -1;
        }

        public Chara(EquipmentSlot slot, string name, string baseString)
        {
            Slot = slot;
            Category = MainItemCategory.Gear;

            Name = name;
            _directory = "chara/equipment/";
            _baseString = baseString;
            //_slotName = _suffixDict[Slot];
            _slotName = Slot.GetShortHandSlot(true);
            _shortVariantString = "v00";
            _longVariantString = "v0000";
            MaterialId = -1;
        }

        public override string GetMtrlPath(XivRace race = XivRace.Hyur_Midlander_Male, string variant = "a")
        {
            return GetMtrlFileName();
        }

        /*
* TODO: I think this also needs body part?
public static string GetSkinTex(XivRace race = XivRace.Hyur_Midlander_Male, string variant = "a")
{
   return "/c" + XivRaces.GetRaceCode(race) + "b0001_" + variant + ".tex";
}
*/
        //TODO: chara/human/c0101/obj/body/b0001/texture/--c0101b0001_d.tex

        // TODO: Mtrl and Tex for fac, etc... and whatever else
        public enum BodyPart
        {
            Face, Etc
        }

        private Dictionary<EquipmentSlot, string> _smallClothesDict = new()
        {
            {EquipmentSlot.Body, "SmallClothes Body" } ,
            {EquipmentSlot.Hands, "SmallClothes Hands" },
            {EquipmentSlot.Legs, "SmallClothes Legs" },
            {EquipmentSlot.Feet, "SmallClothes Feet" }
        };

        public new bool HasSkin()
        {
            return true;
        }
    }
}
