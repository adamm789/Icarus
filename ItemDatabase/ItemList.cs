using ItemDatabase.Enums;
using ItemDatabase.Interfaces;
using Lumina;
using Lumina.Excel.GeneratedSheets;
using Lumina.Text;
using Serilog;
using xivModdingFramework.General.Enums;
using LuminaItem = Lumina.Excel.GeneratedSheets.Item;

namespace ItemDatabase
{
    // TODO: Other mdl items...
    public class ItemList
    {
        GameData _lumina;
        public Dictionary<SeString, List<LuminaItem>> dict = new();

        private List<IItem> _items = new();

        private Dictionary<string, SortedDictionary<string, IItem>> _allItems = new();
        private SortedDictionary<string, IItem> _indoorFurniture = new();
        private SortedDictionary<MainItemCategory, SortedList<string, Item>> OtherItems = new();
        public ItemList(GameData lumina)
        {
            _lumina = lumina;
            BuildList();
        }

        public ItemList(string gamePath)
        {
            _lumina = new GameData(gamePath);
            BuildList();
        }

        /*
        public Dictionary<EquipmentSlot, SortedDictionary<string, IItem>> GetAllItems()
        {
            return _gear;
        }
        */
        public Dictionary<string, SortedDictionary<string, IItem>> GetAllItems()
        {
            return _allItems;
        }

        private void BuildList()
        {
            /*
            _gear[EquipmentSlot.Head] = new();
            _gear[EquipmentSlot.Body] = new();
            _gear[EquipmentSlot.Hands] = new();
            _gear[EquipmentSlot.Legs] = new();
            _gear[EquipmentSlot.Feet] = new();
            _gear[EquipmentSlot.Ears] = new();
            _gear[EquipmentSlot.Neck] = new();
            _gear[EquipmentSlot.Wrists] = new();
            _gear[EquipmentSlot.Ring] = new();
            */

            _allItems[EquipmentSlot.Head.ToString()] = new();
            _allItems[EquipmentSlot.Body.ToString()] = new();
            _allItems[EquipmentSlot.Hands.ToString()] = new();
            _allItems[EquipmentSlot.Legs.ToString()] = new();
            _allItems[EquipmentSlot.Feet.ToString()] = new();
            _allItems[EquipmentSlot.Ears.ToString()] = new();
            _allItems[EquipmentSlot.Neck.ToString()] = new();
            _allItems[EquipmentSlot.Wrists.ToString()] = new();
            _allItems[EquipmentSlot.Ring.ToString()] = new();

            // TODO: Create an actual storage file, so I don't have to do this every time?

            var items = _lumina.GetExcelSheet<LuminaItem>();
            var furnishings = _lumina.GetExcelSheet<HousingFurniture>();

            foreach (var item in items)
            {
                var cat = Item.GetMainItemCategory(item);

                switch (cat)
                {
                    // TODO: I think the only cases here for Lumina.Items are Gear and Null
                    case MainItemCategory.Null:
                        break;
                    case MainItemCategory.Gear:
                        var i = Item.GetItem(item);
                        if (i != null)
                        {
                            AddItem(i);
                        }
                        break;
                    case MainItemCategory.Minions:
                        break;
                    case MainItemCategory.IndoorFurnishings:
                        break;
                    case MainItemCategory.OutdoorFurnishings:
                        break;
                }
            }


            foreach (var f in furnishings)
            {
                var item = new IndoorFurniture(f);
                if (String.IsNullOrWhiteSpace(item.Name)) continue;

                _indoorFurniture.Add(item.Name, item);
            }

            AddBody();
        }

        private void AddItem(IItem item)
        {
            if (item is IGear equip)
            {
                AddEquipment(equip);
            }
        }

        private void AddEquipment(IGear equip)
        {
            var slot = equip.Slot;
            if (EquipmentSlot.Ring.HasFlag(slot))
            {
                slot = EquipmentSlot.Ring;
            }
            if (!_allItems[slot.ToString()].ContainsKey(equip.Name))
            {
                // TODO: "An item with the same key has already been added. Key: [Valentione Emissary's Boots, ItemDatabase.Equipment]"
                _allItems[slot.ToString()].Add(equip.Name, equip);
            }
        }

        public bool Contains(string str)
        {
            var results = Search(str, false);
            return results.Count > 0;
        }

        // TODO: Implement "filter"?
        public List<IItem> Search(string str, bool exactMatch = false)
        {
            var ret = new List<IItem>();
            if (String.IsNullOrWhiteSpace(str))
            {
                return ret;
            }

            foreach (var entry in _allItems.Values)
            {
                foreach (var item in entry)
                {
                    if (item.Value.IsMatch(str))
                    {
                        if (exactMatch)
                        {
                            if (item.Value.Name == str)
                            {
                                ret.Add(item.Value);
                                break;
                            }
                        }
                        else
                        {
                            ret.Add(item.Value);
                        }
                    }
                }
            }

            // Include furniture?
            foreach (var furniture in _indoorFurniture)
            {
                if (furniture.Value.IsMatch(str))
                {
                    ret.Add(furniture.Value);
                }
            }
            return ret;
        }

        public List<IItem> Search(string str, string variantCode = "", bool exactMatch = false)
        {
            var results = Search(str, exactMatch);
            var ret = new List<IItem>();
            foreach (var r in results)
            {
                if (r is IGear g)
                {
                    if (g.VariantCode == variantCode)
                    {
                        ret.Add(r);
                    }
                }
                else if (String.IsNullOrEmpty(variantCode))
                {
                    ret.Add(r);
                }
            }
            return ret;
        }


        /// <summary>
        /// Adds SmallClothes and The Emperor series
        /// </summary>
        private void AddBody()
        {
            // TODO: More research if this has to be manual?
            // i.e. Can I find this information in one of the Lumina ExcelRows?
            var smallClothesBody = new Chara(EquipmentSlot.Body, "SmallClothes Body", "e0000");
            var smallClothesHands = new Chara(EquipmentSlot.Hands, "SmallClothes Hands", "e0000"); ;
            var smallClothesLegs = new Chara(EquipmentSlot.Legs, "SmallClothes Legs", "e0000");
            var smallClothesFeet = new Chara(EquipmentSlot.Feet, "SmallClothes Feet", "e0000");

            AddEquipment(smallClothesBody);
            AddEquipment(smallClothesHands);
            AddEquipment(smallClothesLegs);
            AddEquipment(smallClothesFeet);

            //var emperorFists = new Weapon(e0301);
            //var emperorShield = new Weapon(e0101);
            var emperorHat = new Chara(EquipmentSlot.Head, "The Emperor's New Hat", "e0279");
            var emperorRobe = new Chara(EquipmentSlot.Body, "The Emperor's New Robe", "e0279");
            var emperorGloves = new Chara(EquipmentSlot.Hands, "The Emperor's New Gloves", "e0279"); ;
            var emperorBreeches = new Chara(EquipmentSlot.Legs, "The Emperor's New Breeches", "e0279");
            var emperorFeet = new Chara(EquipmentSlot.Feet, "The Emperor's New Boots", "e0279");

            var emperorBracelet = new Chara(EquipmentSlot.Wrists, "The Emperor's New Bracelet", "a0053");
            var emperorEarrings = new Chara(EquipmentSlot.Ears, "The Emperor's New Earrings", "a0053");
            var emperorNecklace = new Chara(EquipmentSlot.Neck, "The Emperor's New Necklace", "a0053");
            var emperorRing = new Chara(EquipmentSlot.RightRing, "The Emperor's New Ring", "a0053");

            AddEquipment(emperorHat);
            AddEquipment(emperorRobe);
            AddEquipment(emperorGloves);
            AddEquipment(emperorBreeches);
            AddEquipment(emperorFeet);
            AddEquipment(emperorBracelet);
            AddEquipment(emperorEarrings);
            AddEquipment(emperorNecklace);
            AddEquipment(emperorRing);
        }

        /*
        public void Add(ItemData item)
        {
            if (String.IsNullOrWhiteSpace(item.Name))
            {
                return;
            }
            var equipSlot = item.EquipData.EquipSlot;
            var modelMain = item.ModelMain;
            if (equipSlot != EquipmentSlot.None)
            {

                if (!Gear.ContainsKey(equipSlot))
                {
                    Gear.Add(equipSlot, new());
                }
                if (!Gear[equipSlot].ContainsKey(modelMain))
                {
                    Gear[equipSlot].Add(modelMain, new List<ItemData>());
                }

                Gear[equipSlot][modelMain].Add(item);
                return;
            }
            var itemCategory = item.MainCategory;

            if (itemCategory == MainItemCategory.Null)
            {
                return;
            }

            if (!OtherItems.ContainsKey(itemCategory))
            {
                OtherItems.Add(itemCategory, new());
            }

            if (!OtherItems[itemCategory].ContainsKey(item.Name))
            {
                OtherItems[itemCategory].Add(item.Name, item);

            }
        }*/

        /*
        public List<ItemData>? Search(string str)
        {
            var ret = new List<ItemData>();
            foreach (var slot in Gear.Values)
            {
                foreach (var dict in slot.Values)
                {
                    foreach (var item in dict)
                    {
                        if (item.Name.Contains(str, StringComparison.OrdinalIgnoreCase))
                        {
                            ret.Add(item);
                        }
                    }
                }
            }
            return ret;
        }
        */
    }
}