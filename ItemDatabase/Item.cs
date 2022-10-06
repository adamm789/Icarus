using ItemDatabase.Enums;
using ItemDatabase.Interfaces;
using Lumina;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using Lumina.Text;
using xivModdingFramework.General.Enums;
using xivModdingFramework.Textures.Enums;
using LuminaItem = Lumina.Excel.GeneratedSheets.Item;

namespace ItemDatabase
{
    public abstract class Item : IItem
    {
        public string Name { get; protected set; }
        public ulong ModelMain { get; protected set; }
        public MainItemCategory Category { get; protected set; }

        protected LuminaItem? _item;

        public Item()
        {

        }

        public Item(LuminaItem item)
        {
            _item = item;
            Name = _item.Name;
            ModelMain = _item.ModelMain;
            Category = GetMainItemCategory(item);
        }

        public abstract string GetMdlPath();

        public abstract string GetMtrlPath();

        public abstract string GetTexPath(XivTexType type, string variant="");

        public abstract string GetMtrlFileName();

        public static MainItemCategory GetMainItemCategory(LuminaItem item)
        {
            var category = MainItemCategory.Null;
            var equip = item.EquipSlotCategory.Row;
            if (equip != 0)
            {
                category = MainItemCategory.Gear;
            }
            else
            {
                var name = item.Name.ToString();
                if (name == "Outdoor Furnishings")
                {
                    category = MainItemCategory.OutdoorFurnishings;
                }
                else if (name == "Chairs and Beds" ||
                    name == "Tables" ||
                    name == "Tabletop" ||
                    name == "Furnishings" ||
                    name == "Wall-mounted" ||
                    name == "Rugs")
                {
                    category = MainItemCategory.IndoorFurnishings;
                }
                else if (name == "Minions")
                {
                    category = MainItemCategory.Minions;
                }
            }

            return category;
        }

        public static MainItemCategory GetMainItemCategory(HousingFurniture furniture)
        {
            return MainItemCategory.IndoorFurnishings;
        }

        public static bool IsWeapon(LuminaItem item)
        {
            var cat = item.EquipSlotCategory.Value;

            return cat.MainHand == 1 || cat.OffHand == 1;
        }

        public static IItem? GetItem(LuminaItem item)
        {
            if (item == null)
            {
                return null;
            }
            var cat = item.EquipSlotCategory.Value;
            if (cat.MainHand == 1 || cat.OffHand == 1)
            {
                return null;
                //return new Weapon();
            }

            if (Gear.GetSlot(item) != EquipmentSlot.None)
            {
                return new Gear(item);
            }
            return null;
        }

        public virtual bool IsMatch(string str)
        {
            if (String.IsNullOrWhiteSpace(str)) { 
                return false; 
            }
            return Name.Contains(str, StringComparison.OrdinalIgnoreCase);
        }

        protected Dictionary<EquipmentSlot, string> _suffixDict = new()
        {
            { EquipmentSlot.Body, "_top" },
            { EquipmentSlot.Head, "_met" },
            { EquipmentSlot.Hands, "_glv" },
            { EquipmentSlot.Legs, "_dwn" },
            { EquipmentSlot.Feet, "_sho" },
            { EquipmentSlot.Ears, "_ear" },
            { EquipmentSlot.Neck, "_nek" },
            { EquipmentSlot.Wrists, "_wrs" },
            { EquipmentSlot.RightRing, "_rir" },
            { EquipmentSlot.LeftRing, "_rir" }
        };

        protected Dictionary<XivTexType, string> _textureDict = new()
        {
            { XivTexType.Normal, "n" },
            { XivTexType.Specular, "s" },
            { XivTexType.Multi, "m" },
            { XivTexType.Diffuse, "d" }
        };
    }
}
