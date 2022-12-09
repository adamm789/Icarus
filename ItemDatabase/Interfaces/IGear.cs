using ItemDatabase.Enums;
using xivModdingFramework.General.Enums;
using xivModdingFramework.Textures.Enums;

namespace ItemDatabase.Interfaces
{
    public interface IGear : IItem
    {
        new MainItemCategory Category => MainItemCategory.Gear;
        EquipmentSlot Slot { get; }
        string GetMdlFileName(XivRace race = XivRace.Hyur_Midlander_Male);

        string GetMdlPath(XivRace race = XivRace.Hyur_Midlander_Male);
        string GetBaseRaceMdlPath(XivRace race);

        string GetMtrlPath(XivRace race = XivRace.Hyur_Midlander_Male, string variant = "a");
        string GetMtrlFileName(XivRace race = XivRace.Hyur_Midlander_Male, string variant = "a");

        string GetTexPath(XivTexType t, XivRace race = XivRace.Hyur_Midlander_Male, string variant = "a");
        string Code { get; }
        string VariantCode { get; }
        ushort Variant { get; }
        string GetImcPath();
        int MaterialId { get; set; }
    }
}
