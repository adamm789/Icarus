using ItemDatabase.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemDatabase
{
    public static class EquipmentSlotExtensions
    {
        public static string GetShortHandSlot(this EquipmentSlot slot, bool includeUnderscore = false)
        {
            string ret = "";
            switch (slot)
            {
                case EquipmentSlot.None:
                case EquipmentSlot.MainHand:
                case EquipmentSlot.OffHand:
                    break;
                case EquipmentSlot.Head:
                    ret = "met";
                    break;
                case EquipmentSlot.Body:
                    ret = "top";
                    break;
                case EquipmentSlot.Hands:
                    ret = "glv";
                    break;
                case EquipmentSlot.Waist:
                    break;
                case EquipmentSlot.Legs:
                    ret = "dwn";
                    break;
                case EquipmentSlot.Feet:
                    ret = "sho";
                    break;
                case EquipmentSlot.Ears:
                    ret = "ear";
                    break;
                case EquipmentSlot.Neck:
                    ret = "nek";
                    break;
                case EquipmentSlot.Wrists:
                    ret = "wrs";
                    break;
                case EquipmentSlot.RightRing:
                    ret = "rir";
                    break;
                case EquipmentSlot.LeftRing:
                    ret = "ril";
                    break;
                case EquipmentSlot.SoulCrystal:
                    break;
                case EquipmentSlot.Equipment:
                    break;
                case EquipmentSlot.Accessory:
                    break;
                case EquipmentSlot.Weapon:
                    break;
                case EquipmentSlot.Ring:
                    break;
            }
            if (!String.IsNullOrWhiteSpace(ret) && includeUnderscore)
            {
                ret = $"_{ret}";
            }
            return ret;
        }
    }
}
