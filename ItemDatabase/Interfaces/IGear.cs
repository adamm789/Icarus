using ItemDatabase.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.General.Enums;
using xivModdingFramework.Textures.Enums;

namespace ItemDatabase.Interfaces
{
    public interface IGear : IItem
    {
        new MainItemCategory Category => MainItemCategory.Gear;
        EquipmentSlot Slot { get; }
        string GetMdlPath(XivRace race = XivRace.Hyur_Midlander_Male);
        string GetBaseRaceMdlPath(XivRace race);

        string GetMtrlPath(XivRace race = XivRace.Hyur_Midlander_Male, string variant = "a");
        string GetMtrlFileName(XivRace race = XivRace.Hyur_Midlander_Male, string variant = "a");

        string GetTexPath(XivTexType t, XivRace race = XivRace.Hyur_Midlander_Male, string variant="a");
        string Code { get; }
        string VariantCode { get; }
    }
}
