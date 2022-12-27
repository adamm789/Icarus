using ItemDatabase.Enums;
using ItemDatabase.Interfaces;
using Lumina;
using Lumina.Data.Files;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using Lumina.Text;
using Serilog;
using xivModdingFramework.General.Enums;
using xivModdingFramework.SqPack.FileTypes;
using LuminaItem = Lumina.Excel.GeneratedSheets.Item;
using Index = xivModdingFramework.SqPack.FileTypes.Index;
using System.Collections.Immutable;
using System.Reflection.Emit;

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
        private SortedDictionary<string, Dictionary<EquipmentSlot, List<IItem>>> _materialSets = new();


        private Dictionary<string, SortedDictionary<string, IItem>> _gear = new();
        private Dictionary<string, SortedDictionary<string, IItem>> _indoorFurniture2 = new();

        private SortedDictionary<string, GearModelSet> _gearModelSets = new();

        private ITreeNode<(string, IItem?)> _root;
        public ItemList(GameData lumina)
        {
            _lumina = lumina;
        }

        public ITreeNode<(string Header, IItem? Item)> CreateList()
        {
            _root = new ItemTreeNode(("ROOT", null));
            _root.Children.Add(CreateItems());
            return _root;
        }

        public ITreeNode<(string Header, IItem? Item)> CreateItems()
        {
            var ret = new ItemTreeNode(("Gear", null));
            var items = _lumina.GetExcelSheet<LuminaItem>();

            var dict = new SortedDictionary<string, SortedSet<ITreeNode<(string, IItem?)>>>();

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
                        if (i is IGear gear)
                        {
                            var t = CreateItem(i);
                            SortedSet<ITreeNode<(string, IItem?)>>? parent;
                            if (EquipmentSlot.Ring.HasFlag(gear.Slot))
                            {
                                if (!dict.ContainsKey(EquipmentSlot.Ring.ToString()))
                                {
                                    dict[EquipmentSlot.Ring.ToString()] = new();
                                }
                                parent = dict[EquipmentSlot.Ring.ToString()];
                            }
                            else
                            {
                                if (!dict.ContainsKey(gear.Slot.ToString()))
                                {
                                    dict[gear.Slot.ToString()] = new();
                                }

                                parent = dict[gear.Slot.ToString()];
                            }
                            if (parent != null)
                            {
                                parent.Add(t);
                            }
                            else
                            {
                                Log.Error($"Could not add {gear.Name}");
                            }
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

            ret.Children.Add(new ItemTreeNode((EquipmentSlot.Head.ToString(), null)) { Children = dict[EquipmentSlot.Head.ToString()] });
            ret.Children.Add(new ItemTreeNode((EquipmentSlot.Body.ToString(), null)) { Children = dict[EquipmentSlot.Body.ToString()] });
            ret.Children.Add(new ItemTreeNode((EquipmentSlot.Hands.ToString(), null)) { Children = dict[EquipmentSlot.Hands.ToString()] });
            ret.Children.Add(new ItemTreeNode((EquipmentSlot.Legs.ToString(), null)) { Children = dict[EquipmentSlot.Legs.ToString()] });
            ret.Children.Add(new ItemTreeNode((EquipmentSlot.Feet.ToString(), null)) { Children = dict[EquipmentSlot.Feet.ToString()] });
            ret.Children.Add(new ItemTreeNode((EquipmentSlot.Ears.ToString(), null)) { Children = dict[EquipmentSlot.Ears.ToString()] });
            ret.Children.Add(new ItemTreeNode((EquipmentSlot.Neck.ToString(), null)) { Children = dict[EquipmentSlot.Neck.ToString()] });
            ret.Children.Add(new ItemTreeNode((EquipmentSlot.Wrists.ToString(), null)) { Children = dict[EquipmentSlot.Wrists.ToString()] });
            ret.Children.Add(new ItemTreeNode((EquipmentSlot.Ring.ToString(), null)) { Children = dict[EquipmentSlot.Ring.ToString()] });

            return ret;
        }

        public ITreeNode<(string Header, IItem? Item)>? CreateItem(IItem? item)
        {
            if (item is IGear gear)
            {
                var imcPath = gear.GetImcPath();
                var part = GetPart(gear.Slot);
                if (part != -1)
                {
                    try
                    {
                        var imcFile = _lumina.GetFile<ImcFile>(imcPath);
                        var materialId = imcFile.GetVariant(part, gear.Variant - 1).MaterialId;
                        gear.MaterialId = materialId;
                    }
                    catch (Exception)
                    {
                        Log.Error($"Could not get imc file for {gear.Name}");
                    }

                    if (!_gearModelSets.ContainsKey(gear.Code))
                    {
                        _gearModelSets.Add(gear.Code, new GearModelSet(gear.Variant));
                    }
                    _gearModelSets[gear.Code].Add(gear);

                    if (!_materialSets.ContainsKey(gear.Code))
                    {
                        _materialSets[gear.Code] = new();
                    }
                    if (!_materialSets[gear.Code].ContainsKey(gear.Slot))
                    {
                        _materialSets[gear.Code][gear.Slot] = new();
                    }
                    _materialSets[gear.Code][gear.Slot].Add(gear);
                }
                return new ItemTreeNode((gear.Name, gear));
            }
            return null;
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

        public Dictionary<string, Dictionary<XivRace, SortedDictionary<string, IItem>>> GetChara()
        {
            var ret = new Dictionary<string, Dictionary<XivRace, SortedDictionary<string, IItem>>>();

            return ret;
        }

        public Dictionary<string, Dictionary<string, SortedDictionary<string, IItem>>> GetAllItems2()
        {
            var ret = new Dictionary<string, Dictionary<string, SortedDictionary<string, IItem>>>();
            ret["Gear"] = _gear;

#if DEBUG
            ret["Indoor Furniture"] = _indoorFurniture2;
#endif
            return ret;
        }

        private Dictionary<string, SortedDictionary<string, IItem>> GetGear()
        {
            var ret = new Dictionary<string, SortedDictionary<string, IItem>>();
            var items = _lumina.GetExcelSheet<LuminaItem>();

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

            return ret;
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

            foreach (var item in items)
            {
                if (item.Name.ToString().Contains("emperor", StringComparison.OrdinalIgnoreCase))
                {

                }
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

            AddIndoorFurniture();
            AddCharacter();
            AddBody();
        }

        private void AddIndoorFurniture()
        {
#if DEBUG
            Log.Debug($"Adding indoor furniture");
            var furnishings = _lumina.GetExcelSheet<HousingFurniture>();
            _indoorFurniture2["Indoor Furnishings"] = new();
            _indoorFurniture2["Tables"] = new();
            _indoorFurniture2["Tabletop"] = new();
            _indoorFurniture2["Wall-mounted"] = new();
            _indoorFurniture2["Rugs"] = new();

            foreach (var f in furnishings)
            {
                var item = new IndoorFurniture(f);
                if (String.IsNullOrWhiteSpace(item.Name)) continue;

                _indoorFurniture.Add(item.Name, item);
                string cat;
                switch (f.HousingItemCategory)
                {
                    case 12:
                        cat = "Indoor Furnishings";
                        break;
                    case 13:
                        cat = "Tables";
                        break;
                    case 14:
                        cat = "Tabletop";
                        break;
                    case 15:
                        cat = "Wall-mounted";
                        break;
                    case 16:
                        cat = "Rugs";
                        break;
                    default:
                        cat = f.HousingItemCategory.ToString();
                        break;
                }
                if (!_indoorFurniture2.ContainsKey(cat))
                {
                    _indoorFurniture2[cat] = new();
                }
                _indoorFurniture2[cat].Add(item.Name, item);

            }
            _allItems["Indoor Furniture"] = _indoorFurniture;
#endif
        }

        private void AddCharacter()
        {
#if DEBUG
            Log.Debug($"Adding character");
            var frameworkPath = Path.Combine(_lumina.DataPath.FullName, "ffxiv");
            var index = new Index(new DirectoryInfo(frameworkPath));
            // Hair
            var hair = new SortedDictionary<string, IItem>();
            foreach (var race in XivRaces.PlayableRaces)
            {
                var code = race.GetRaceCode();
                for (var i = 0; i < 300; i++)
                {
                    var itemNum = i.ToString().PadLeft(4, '0');
                    var folder = $"chara/human/c{code}/obj/hair/h{itemNum}/model";
                    // TODO: Add "chara" items
                    // TODO: Find better way to find "chara" items
                    var b = Task.Run(() => index.FolderExists(folder, XivDataFile._04_Chara)).Result;
                    if (b)
                    {
                        var chara = new Chara(EquipmentSlot.Head, $"{race} - Hair {itemNum}", itemNum);
                        if (!hair.ContainsKey(itemNum))
                        {
                            hair.Add(itemNum, chara);
                        }
                    }
                }
            }
            _allItems["Hair"] = hair;
#endif
        }

        public List<IGear>? GetSharedModels(IGear gear)
        {
            if (_gearModelSets.ContainsKey(gear.Code))
            {
                return _gearModelSets[gear.Code].GetSlot(gear.Slot);
            }
            return null;
        }

        private void AddItem(IItem item)
        {
            if (item is IGear gear)
            {
                var imcPath = gear.GetImcPath();
                var part = GetPart(gear.Slot);
                if (part != -1)
                {
                    try
                    {
                        var imcFile = _lumina.GetFile<ImcFile>(imcPath);
                        var materialId = imcFile.GetVariant(part, gear.Variant - 1).MaterialId;
                        gear.MaterialId = materialId;
                    }
                    catch (Exception)
                    {
                        Log.Error($"Could not get imc file for {gear.Name}");
                    }
                }
                AddEquipment(gear);
                if (!_gearModelSets.ContainsKey(gear.Code))
                {
                    _gearModelSets.Add(gear.Code, new GearModelSet(gear.Variant));
                }
                _gearModelSets[gear.Code].Add(gear);

                if (!_materialSets.ContainsKey(gear.Code))
                {
                    _materialSets[gear.Code] = new();
                }
                if (!_materialSets[gear.Code].ContainsKey(gear.Slot))
                {
                    _materialSets[gear.Code][gear.Slot] = new();
                }
                _materialSets[gear.Code][gear.Slot].Add(gear);
            }
            else
            {

            }
        }

        private int GetPart(EquipmentSlot slot)
        {
            switch (slot)
            {
                case EquipmentSlot.Head: return 0;
                case EquipmentSlot.Body: return 1;
                case EquipmentSlot.Hands: return 2;
                case EquipmentSlot.Legs: return 3;
                case EquipmentSlot.Feet: return 4;

                default: return 0;
            }
        }

        private void AddEquipment(IGear gear)
        {
            var slot = gear.Slot;
            if (EquipmentSlot.Ring.HasFlag(slot))
            {
                slot = EquipmentSlot.Ring;
            }
            if (!_allItems[slot.ToString()].ContainsKey(gear.Name))
            {
                // TODO: "An item with the same key has already been added. Key: [Valentione Emissary's Boots, ItemDatabase.Equipment]"
                _allItems[slot.ToString()].Add(gear.Name, gear);
            }
            if (!_gear.ContainsKey(slot.ToString()))
            {
                _gear[slot.ToString()] = new();
            }
            _gear[slot.ToString()].Add(gear.Name, gear);

        }

        public bool Contains(string str)
        {
            var results = Search(str, false);
            return results.Count > 0;
        }

        public int GetNumMaterialSets(IItem item)
        {
            if (item is IGear gear)
            {
                _materialSets.TryGetValue(gear.Code, out var set);
                if (set != null)
                {
                    set.TryGetValue(gear.Slot, out var list);
                    if (list != null)
                    {
                        return list.Count;
                    }
                }
            }
            return 0;
        }

        public List<IItem>? GetMaterialSet(IItem item)
        {
            if (item is IGear gear)
            {
                _materialSets.TryGetValue(gear.Code, out var set);
                if (set != null)
                {
                    set.TryGetValue(gear.Slot, out var list);
                    return list;
                }
            }
            return new List<IItem>();
        }

        private List<IItem> Search(ITreeNode<(string header, IItem? item)> node, Predicate<IItem> pred)
        {
            if (node.Value.item != null)
            {
                if (pred(node.Value.item))
                {
                    return new List<IItem>() { node.Value.item };
                }
                return new List<IItem>();
            }
            else
            {
                var ret = new List<IItem>();
                foreach (var child in node.Children)
                {
                    ret.AddRange(Search(child, pred));
                }
                return ret;
            }
        }

        public List<IItem> Search(string str, bool exactMatch = false)
        {
            if (String.IsNullOrWhiteSpace(str))
            {
                return new List<IItem>();
            }

            Predicate<IItem> pred;
            if (exactMatch)
            {
                pred = i => i.Name == str;
            }
            else
            {
                pred = i => i.Name.Contains(str);
            }
            var ret = Search(_root, pred);

            /*
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
            */
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
            /*
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
            */
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