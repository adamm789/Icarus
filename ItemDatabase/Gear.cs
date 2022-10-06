using ItemDatabase;
using ItemDatabase.Enums;
using ItemDatabase.Interfaces;
using Lumina;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using xivModdingFramework.General.Enums;
using xivModdingFramework.Textures.Enums;
using LuminaItem = Lumina.Excel.GeneratedSheets.Item;

namespace ItemDatabase
{
    public class Gear : Item, IGear
    {
        public EquipmentSlot Slot { get; protected set; }
        public string Code { get; protected set; } = "";
        public string VariantCode { get; protected set; }

        protected ushort _base;
        protected ushort _variant;

        /// <summary>
        /// [e,a][0-9]{4}
        /// </summary>
        protected string _baseString;
        protected string _directory;

        /// <summary>
        /// Prepended with underscore
        /// </summary>
        protected string _slotName;

        protected string _shortVariantString;
        protected string _longVariantString;

        public Gear()
        {

        }

        public Gear(LuminaItem item) : base(item)
        {
            Slot = GetMainSlot(_item.EquipSlotCategory.Value);

            if (Slot == EquipmentSlot.None)
            {
                return;
            }

            _base = (ushort)ModelMain;
            _variant = (ushort)(ModelMain >> 16);

            _slotName = _suffixDict[Slot];
            _shortVariantString = "v" + _variant.ToString().PadLeft(2, '0');
            _longVariantString = "v" + _variant.ToString().PadLeft(4, '0');

            if (EquipmentSlot.Equipment.HasFlag(Slot))
            {
                _baseString = "e" + _base.ToString().PadLeft(4, '0');
                _directory = "chara/equipment/";
            }
            else
            {
                _baseString = "a" + _base.ToString().PadLeft(4, '0');
                _directory = "chara/accessory/";
            }
            Code = _baseString;
            VariantCode = $"{_baseString}{_slotName}_v{_variant}";
        }

        public override string GetMdlPath()
        {
            return GetMdlPath();
        }

        public virtual string GetMdlPath(XivRace race = XivRace.Hyur_Midlander_Male)
        {
            return $"{_directory}{_baseString}/model/{GetRaceCode(race)}{_baseString}{_slotName}.mdl";
            //return _directory + _baseString + "/model/" + GetRaceCode(race) + _baseString + _slotName + ".mdl";
        }
        public virtual string GetBaseRaceMdlPath(XivRace race)
        {
            var baseRace = GetBaseModelRace(race);
            return GetMdlPath(baseRace);
        }

        public override string GetMtrlPath()
        {
            return GetMtrlFileName();
        }

        public override string GetMtrlFileName()
        {
            return GetMtrlFileName();
        }
        public virtual string GetMtrlFileName(XivRace race = XivRace.Hyur_Midlander_Male, string variant = "a")
        {
            return $"/mt_{GetRaceCode(race)}{_baseString}{_slotName}_{variant}.mtrl";
            //return "/mt_" + GetRaceCode(race) + _baseString + _slotName + "_" + variant + ".mtrl";
        }

        public virtual string GetMtrlPath(XivRace race = XivRace.Hyur_Midlander_Male, string variant = "a")
        {
            return $"{_directory}{_baseString}/material/{_longVariantString}{GetMtrlFileName(race, variant)}";
            //return _directory + _baseString + "/material/" + _longVariantString + GetMtrlFileName(race, variant);
        }

        public override string GetTexPath(XivTexType t, string variant="")
        {
            return GetTexPath(t, XivRace.Hyur_Midlander_Male, variant);
        }
        public virtual string GetTexPath(XivTexType t, XivRace race = XivRace.Hyur_Midlander_Male, string variant = "a")
        {
            if (t is XivTexType.ColorSet)
            {
                return "";
            }
            string v = "";
            if (variant != "a" && !String.IsNullOrEmpty(variant))
            {
                v = $"_{variant}";
            }
            //return $"{_directory}{_baseString}/texture/{_shortVariantString}_{GetRaceCode(race)}{_baseString}{_slotName}{v}_{_textureDict[t]}.tex";
            return $"{_directory}{_baseString}/texture/v01_{GetRaceCode(race)}{_baseString}{_slotName}{v}_{_textureDict[t]}.tex";
            //return _directory + _baseString + "/texture/" + _shortVariantString + "_" + GetRaceCode(race) + _baseString + _slotName + "_" + _textureDict[t] + ".tex";
        }

        public override bool IsMatch(string str)
        {
            if (String.IsNullOrWhiteSpace(str))
            {
                return false;
            }
            // TODO: Handle the case when the full path is provided
            if (str.Contains(".mtrl") || str.Contains(".tex") || str.Contains(".mdl"))
            {
                if (str.Contains($"{_baseString}{_slotName}"))
                {
                    return true;
                }
            }
            return base.IsMatch(str) || _baseString.Contains(str, StringComparison.OrdinalIgnoreCase)
                || $"{_baseString}{_slotName}".Contains(str, StringComparison.OrdinalIgnoreCase);
        }

        public static EquipmentSlot GetSlot(LuminaItem item)
        {
            return GetMainSlot(item.EquipSlotCategory.Value);
        }

        public static EquipmentSlot GetSlot(EquipSlotCategory cat)
        {
            var slot = EquipmentSlot.None;
            if (cat.Body == 1) slot |= EquipmentSlot.Body;
            if (cat.Head == 1) slot |= EquipmentSlot.Head;
            if (cat.Gloves == 1) slot |= EquipmentSlot.Hands;
            if (cat.Legs == 1) slot |= EquipmentSlot.Legs;
            if (cat.Feet == 1) slot |= EquipmentSlot.Feet;
            if (cat.Ears == 1) slot |= EquipmentSlot.Ears;
            if (cat.Neck == 1) slot |= EquipmentSlot.Neck;
            if (cat.Wrists == 1) slot |= EquipmentSlot.Wrists;
            if (cat.FingerL == 1) slot |= EquipmentSlot.LeftRing;
            if (cat.FingerR == 1 ) slot |= EquipmentSlot.RightRing;
            if (cat.MainHand == 1) slot |= EquipmentSlot.MainHand;
            if (cat.OffHand == 1) slot |= EquipmentSlot.OffHand;

            return slot;
        }

        public static EquipmentSlot GetMainSlot(EquipSlotCategory cat)
        {
            if (cat.Body == 1)
            {
                // Evaluate this first for equipment that covers multiple parts
                return EquipmentSlot.Body;
            }
            else if (cat.Head == 1)
            {
                return EquipmentSlot.Head;
            }
            else if (cat.Gloves == 1)
            {
                return EquipmentSlot.Hands;
            }
            else if (cat.Legs == 1)
            {
                return EquipmentSlot.Legs;
            }
            else if (cat.Feet == 1)
            {
                return EquipmentSlot.Feet;
            }
            else if (cat.Ears == 1)
            {
                return EquipmentSlot.Ears;
            }
            else if (cat.Neck == 1)
            {
                return EquipmentSlot.Neck;
            }
            else if (cat.Wrists == 1)
            {
                return EquipmentSlot.Wrists;
            }
            /*
            else if (cat.FingerL == 1)
            {
                Changing the mdl of a left ring doesn't seem to do anything meaningful?
                Equipreturn EquipSlot.LeftRing;
                Suffix = "_ril";
            }
            */
            else if (cat.FingerR == 1 || cat.FingerL == 1)
            {
                return EquipmentSlot.RightRing;
            }
            else if (cat.MainHand == 1)
            {
                return EquipmentSlot.MainHand;
            }
            else if (cat.OffHand == 1)
            {
                return EquipmentSlot.OffHand;
            }
            else
            {
                // SoulCrystal or Waist or anything else
                return EquipmentSlot.None;
            }
        }

        protected string GetRaceCode(XivRace race)
        {
            return "c" + XivRaces.GetRaceCode(race);
        }

        protected XivRace GetBaseModelRace(XivRace race)
        {
            switch (race)
            {
                case XivRace.Hyur_Midlander_Male:
                case XivRace.Hyur_Midlander_Male_NPC:
                case XivRace.Elezen_Male:
                case XivRace.Elezen_Male_NPC:
                case XivRace.Miqote_Male:
                case XivRace.Miqote_Male_NPC:
                case XivRace.AuRa_Male:
                case XivRace.AuRa_Male_NPC:
                case XivRace.Viera_Male:
                case XivRace.Viera_Male_NPC:
                case XivRace.Hyur_Highlander_Male:
                case XivRace.Hyur_Highlander_Male_NPC:
                    if ((race == XivRace.Hyur_Highlander_Male || race == XivRace.Hyur_Highlander_Male_NPC) & EquipmentSlot.Body.HasFlag(Slot))
                    {
                        return XivRace.Hyur_Highlander_Male;
                    }
                    else
                    {
                        return XivRace.Hyur_Midlander_Male;
                    }
                case XivRace.Hrothgar_Male:
                case XivRace.Hrothgar_Male_NPC:
                case XivRace.Roegadyn_Male:
                case XivRace.Roegadyn_Male_NPC:
                    return XivRace.Roegadyn_Male;
                case XivRace.Lalafell_Male:
                case XivRace.Lalafell_Male_NPC:
                case XivRace.Lalafell_Female:
                case XivRace.Lalafell_Female_NPC:
                    return XivRace.Lalafell_Male;
                case XivRace.Hyur_Midlander_Female:
                case XivRace.Hyur_Midlander_Female_NPC:
                case XivRace.Hyur_Highlander_Female:
                case XivRace.Hyur_Highlander_Female_NPC:
                case XivRace.Elezen_Female:
                case XivRace.Elezen_Female_NPC:
                case XivRace.Miqote_Female:
                case XivRace.Miqote_Female_NPC:
                case XivRace.Roegadyn_Female:
                case XivRace.Roegadyn_Female_NPC:
                case XivRace.AuRa_Female:
                case XivRace.AuRa_Female_NPC:
                case XivRace.Viera_Female:
                case XivRace.Viera_Female_NPC:
                    return XivRace.Hyur_Midlander_Female;
                default:
                    var err = "Unsupported race: " + race.ToString();
                    throw new ArgumentException(err);
            }
        }
    }
}
