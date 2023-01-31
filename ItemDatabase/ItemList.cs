using ItemDatabase.Enums;
using ItemDatabase.Interfaces;
using Lumina;
using Lumina.Data.Files;
using Lumina.Excel;
using Lumina.Text;
using Serilog;
using xivModdingFramework.General.Enums;
using LuminaItem = Lumina.Excel.GeneratedSheets.Item;
using CharaMakeType = ItemDatabase.Lumina.CharaMakeType;
using ItemDatabase.Characters;
using Lumina.Excel.GeneratedSheets;
using Lumina.Excel.Exceptions;

namespace ItemDatabase
{
    // TODO: Other mdl items...
    public class ItemList
    {
        GameData _lumina;
        public Dictionary<SeString, List<LuminaItem>> dict = new();

        private SortedDictionary<string, Dictionary<EquipmentSlot, List<IItem>>> _materialSets = new();
        private SortedDictionary<string, GearModelSet> _gearModelSets = new();

        private ItemTreeNode _root;
        public ItemList(GameData lumina)
        {
            _lumina = lumina;
        }

        public ItemTreeNode CreateList()
        {
            _root = new ItemTreeNode(("ROOT", null));
            Log.Debug($"Starting list.");

            var items = CreateItems();
            var character = CreateCharacterLoop();
            var furniture = CreateFurniture();

            _root.AddChild(items);
            _root.AddChild(character);

            if (furniture != null) _root.AddChild(furniture);

            Log.Debug("Ending list");
            return _root;
        }

        public ITreeNode<(string Header, IItem? Item)> CreateCharacterLoop()
        {
            Log.Debug($"Adding Charater Items via loop.");
            var ret = new ItemTreeNode(("Character", null));

            var bodies = new ItemTreeNode(("Body", null));
            var faces = new ItemTreeNode(("Faces", null));
            var hairstyles = new ItemTreeNode(("Hair", null));

            var faceDict = new Dictionary<XivRace, SortedDictionary<int, IItem>>();
            var hairDict = new Dictionary<XivRace, SortedDictionary<int, IItem>>();
            var bodyDict = new Dictionary<XivRace, SortedDictionary<int, IItem>>();

            for (var i = 0; i < 300; i++)
            {
                foreach (var race in XivRaces.PlayableRaces)
                {
                    if (!faceDict.ContainsKey(race))
                    {
                        faceDict.Add(race, new SortedDictionary<int, IItem>());
                    }
                    if (!hairDict.ContainsKey(race))
                    {
                        hairDict.Add(race, new SortedDictionary<int, IItem>());
                    }
                    if (!bodyDict.ContainsKey(race))
                    {
                        bodyDict.Add(race, new SortedDictionary<int, IItem>());
                    }

                    var face = new CharacterFace(race, i);
                    if (_lumina.FileExists(face.GetMdlPath()) && !faceDict[race].ContainsKey(i))
                    {
                        faceDict[race].Add(i, face);
                    }

                    var hair = new CharacterHair(race, i);
                    if (_lumina.FileExists(hair.GetMdlPath()) && !hairDict[race].ContainsKey(i))
                    {
                        hairDict[race].Add(i, hair);
                    }


                    var body = new CharacterBody(race, i);
                    if (_lumina.FileExists(body.GetMdlPath()) && !bodyDict[race].ContainsKey(i))
                    {
                        bodyDict[race].Add(i, body);
                    }

                }
            }
            foreach (var race in XivRaces.PlayableRaces)
            {
                if (faceDict.ContainsKey(race))
                {
                    var values = faceDict[race].Values;
                    var child = new ItemTreeNode((race.GetDisplayName(), null));
                    foreach (var value in values)
                    {
                        child.AddChild(value);
                    }
                    faces.AddChild(child);
                }
                if (hairDict.ContainsKey(race))
                {
                    var values = hairDict[race].Values;
                    var child = new ItemTreeNode((race.GetDisplayName(), null));
                    foreach (var value in values)
                    {
                        child.AddChild(value);
                    }
                    hairstyles.AddChild(child);
                }
                if (bodyDict.ContainsKey(race))
                {
                    var values = bodyDict[race].Values;
                    var child = new ItemTreeNode((race.GetDisplayName(), null));
                    foreach (var value in values)
                    {
                        child.AddChild(value);
                    }
                    bodies.AddChild(child);
                }
            }

            ret.AddChild(bodies);
            ret.AddChild(faces);
            ret.AddChild(hairstyles);
            (var tails, var ears) = GetTailsAndEars();
            ret.AddChild(tails);
            ret.AddChild(ears);
            return ret;
        }

        public (ItemTreeNode tails, ItemTreeNode ears) GetTailsAndEars(ExcelSheet<CharaMakeType>? sheet = null)
        {
            if (sheet == null)
            {
                sheet = _lumina.GetExcelSheet<CharaMakeType>();
            }
            var tailDict = new Dictionary<XivRace, List<IItem>>();
            var earDict = new Dictionary<XivRace, List<IItem>>();

            for (var i = 0; i < sheet.RowCount; i++)
            {
                var row = sheet.GetRow((uint)i);
                var tailColumn = -1;
                var earColumn = -1;


                for (var j = 0; j < row.Menu.Length; j++)
                {
                    var num = row.Menu[j].Row;

                    if (num == 223 || num == 226 || num == 253 || num == 257 || num == 1000)
                    {
                        tailColumn = j;
                    }
                    else if (num == 1033 || num == 1016)
                    {
                        earColumn = j;
                    }
                }
                var race = GetXivRace(row.Tribe.Row, row.Gender);
                if (tailColumn > 0)
                {
                    var numTails = row.SubMenuParam[tailColumn].Where(x => x > 0).Count();
                    if (!tailDict.ContainsKey(race))
                    {
                        tailDict.Add(race, new List<IItem>());
                        for (var k = 0; k < numTails; k++)
                        {
                            var tail = new CharacterTail(race, k + 1);
                            tailDict[race].Add(tail);
                        }
                    }
                }
                if (earColumn > 0)
                {
                    var numEars = row.SubMenuParam[earColumn].Where(x => x > 0).Count();
                    if (!earDict.ContainsKey(race))
                    {
                        earDict.Add(race, new List<IItem>());
                        for (var k = 0; k < numEars; k++)
                        {
                            var ear = new CharacterEar(race, k + 1);
                            earDict[race].Add(ear);
                        }
                    }
                }
            }
            var tails = new ItemTreeNode(("Tails", null));
            foreach (var kvp in tailDict)
            {
                var race = kvp.Key;
                var itemList = kvp.Value;
                var child = new ItemTreeNode((race.GetDisplayName(), null));
                tails.AddChild(child);
                foreach (var item in itemList)
                {
                    child.AddChild(item);
                }
            }

            var ears = new ItemTreeNode(("Ears", null));
            foreach (var kvp in earDict)
            {
                var race = kvp.Key;
                var itemList = kvp.Value;
                var child = new ItemTreeNode((race.GetDisplayName(), null));
                ears.AddChild(child);
                foreach (var item in itemList)
                {
                    child.AddChild(item);
                }
            }
            return (tails, ears);
        }


        // TODO: Clean up method to obtain Character items
        public ITreeNode<(string Header, IItem? Item)> CreateCharacter()
        {
            var ret = new ItemTreeNode(("Character", null));

            var sheet = _lumina.GetExcelSheet<CharaMakeType>();
            var faceDict = new Dictionary<XivRace, SortedDictionary<int, IItem>>();
            var tailDict = new Dictionary<XivRace, List<IItem>>();
            var earDict = new Dictionary<XivRace, List<IItem>>();

            for (var i = 0; i < sheet.RowCount; i++)
            {
                var row = sheet.GetRow((uint)i);
                var faceColumn = -1;
                var tailColumn = -1;
                var earColumn = -1;
                var hairColumn = -1;

                for (var j = 0; j < row.Menu.Length; j++)
                {
                    var num = row.Menu[j].Row;
                    if (num == 238 || num == 252)
                    {
                        if (faceColumn != -1)
                        {

                        }
                        faceColumn = j;
                    }
                    else if (num == 223 || num == 226 || num == 253 || num == 257 || num == 1000)
                    {
                        tailColumn = j;
                    }
                    else if (num == 1033 || num == 1016)
                    {
                        earColumn = j;
                    }
                    else if (num == 234)
                    {
                        hairColumn = j;
                    }
                }
                var race = GetXivRace(row.Tribe.Row, row.Gender);

                if (faceColumn > 0)
                {
                    try
                    {
                        var numFaces = row.SubMenuParam[faceColumn].Where(x => x > 0).Count();
                        if (!faceDict.ContainsKey(race))
                        {
                            faceDict.Add(race, new SortedDictionary<int, IItem>());
                            for (var k = 0; k < numFaces; k++)
                            {
                                var face = new CharacterFace(race, k + 1);
                                faceDict[race].Add(k + 1, face);
                            }
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
                if (tailColumn > 0)
                {
                    var numTails = row.SubMenuParam[tailColumn].Where(x => x > 0).Count();
                    if (!tailDict.ContainsKey(race))
                    {
                        tailDict.Add(race, new List<IItem>());
                        for (var k = 0; k < numTails; k++)
                        {
                            var tail = new CharacterTail(race, k + 1);
                            tailDict[race].Add(tail);
                        }
                    }
                }
                if (earColumn > 0)
                {
                    var numEars = row.SubMenuParam[earColumn].Where(x => x > 0).Count();
                    if (!earDict.ContainsKey(race))
                    {
                        earDict.Add(race, new List<IItem>());
                        for (var k = 0; k < numEars; k++)
                        {
                            var ear = new CharacterEar(race, k + 1);
                            earDict[race].Add(ear);
                        }
                    }
                }
                if (hairColumn > 0)
                {
                    var hairsss = row.SubMenuParam[hairColumn].Where(x => x > 0);
                    var numHair = row.SubMenuParam[hairColumn].Where(x => x > 0).Count();
                }
            }

            // TODO: Looping through BNpc still doesn't get all of the faces...
            // Should I just iterate like TexTools does...?
            var bnpc = _lumina.GetExcelSheet<BNpcCustomize>();
            for (var i = 0; i < bnpc.RowCount; i++)
            {
                var row = bnpc.GetRow((uint)i);

                if (row.Tribe.Row <= 0) continue;
                var race = GetXivRace(row.Tribe.Row, row.Gender);

                if (row.Face > 7 && !faceDict[race].ContainsKey(row.Face))
                {
                    var face = new CharacterFace(race, row.Face);
                    faceDict[race].Add(row.Face, face);

                }
            }

            var faces = new ItemTreeNode(("Faces", null));
            foreach (var kvp in faceDict)
            {
                var race = kvp.Key;
                var itemList = kvp.Value;
                var child = new ItemTreeNode((race.GetDisplayName(), null));
                faces.AddChild(child);
                foreach (var item in itemList.Values)
                {
                    child.AddChild(item);
                }
            }

            var tails = new ItemTreeNode(("Tails", null));
            foreach (var kvp in tailDict)
            {
                var race = kvp.Key;
                var itemList = kvp.Value;
                var child = new ItemTreeNode((race.GetDisplayName(), null));
                tails.AddChild(child);
                foreach (var item in itemList)
                {
                    child.AddChild(item);
                }
            }

            var ears = new ItemTreeNode(("Ears", null));
            foreach (var kvp in earDict)
            {
                var race = kvp.Key;
                var itemList = kvp.Value;
                var child = new ItemTreeNode((race.GetDisplayName(), null));
                ears.AddChild(child);
                foreach (var item in itemList)
                {
                    child.AddChild(item);
                }
            }

            ret.AddChild(faces);
            ret.AddChild(tails);
            ret.AddChild(ears);

            return ret;
        }

        public XivRace GetXivRace(uint tribe, int gender)
        {
            if (tribe == 1)
            {
                if (gender == 0) return XivRace.Hyur_Midlander_Male;
                else return XivRace.Hyur_Midlander_Female;
            }
            else if (tribe == 2)
            {
                if (gender == 0) return XivRace.Hyur_Highlander_Male;
                else return XivRace.Hyur_Highlander_Female;
            }
            else if (tribe == 3 || tribe == 4)
            {
                if (gender == 0) return XivRace.Elezen_Male;
                else return XivRace.Elezen_Female;
            }
            else if (tribe == 5 || tribe == 6)
            {
                if (gender == 0) return XivRace.Lalafell_Male;
                else return XivRace.Lalafell_Female;
            }
            else if (tribe == 7 || tribe == 8)
            {
                if (gender == 0) return XivRace.Miqote_Male;
                else return XivRace.Miqote_Female;
            }
            else if (tribe == 9 || tribe == 10)
            {
                if (gender == 0) return XivRace.Roegadyn_Male;
                else return XivRace.Roegadyn_Female;
            }
            else if (tribe == 11 || tribe == 12)
            {
                if (gender == 0) return XivRace.AuRa_Male;
                else return XivRace.AuRa_Female;
            }
            else if (tribe == 13 || tribe == 14)
            {
                if (gender == 0) return XivRace.Hrothgar_Male;
                else return XivRace.Hrothgar_Female;
            }
            else if (tribe == 15 || tribe == 16)
            {
                if (gender == 0) return XivRace.Viera_Male;
                else return XivRace.Viera_Female;
            }

            throw new ArgumentException();
        }

        public ITreeNode<(string Header, IItem? Item)> CreateItems()
        {
            var ret = new ItemTreeNode(("Gear", null));
            ExcelSheet<LuminaItem> items;
            try
            {
                items = _lumina.GetExcelSheet<LuminaItem>();
            }
            catch (ExcelSheetColumnChecksumMismatchException ex)
            {
                Log.Error($"Could not add gear.");
                Log.Error(ex.Message);
                return ret;
            }

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
                            if (parent != null && t != null)
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

            dict[EquipmentSlot.Body.ToString()].Add(new ItemTreeNode(("SmallClothes Body", new Chara(EquipmentSlot.Body, "SmallClothes Body", "e0000"))));
            dict[EquipmentSlot.Hands.ToString()].Add(new ItemTreeNode(("SmallClothes Hands", new Chara(EquipmentSlot.Hands, "SmallClothes Hands", "e0000"))));
            dict[EquipmentSlot.Legs.ToString()].Add(new ItemTreeNode(("SmallClothes Legs", new Chara(EquipmentSlot.Legs, "SmallClothes Legs", "e0000"))));
            dict[EquipmentSlot.Feet.ToString()].Add(new ItemTreeNode(("SmallClothes Feet", new Chara(EquipmentSlot.Feet, "SmallClothes Feet", "e0000"))));

            var equipmentOrder = new List<EquipmentSlot>()
            {
                EquipmentSlot.Head, EquipmentSlot.Body, EquipmentSlot.Hands, EquipmentSlot.Legs, EquipmentSlot.Feet,
                EquipmentSlot.Ears, EquipmentSlot.Neck, EquipmentSlot.Wrists, EquipmentSlot.Ring
            };
            foreach (var slot in equipmentOrder)
            {
                var node = new ItemTreeNode((slot.ToString(), null));
                node.AddChildren(dict[slot.ToString()]);
                ret.AddChild(node);
            }

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

        public ITreeNode<(string Header, IItem? Item)>? CreateFurniture()
        {
            Log.Debug($"Adding Housing Furniture");
            try
            {
                var ret = new ItemTreeNode(("Indoor Furniture", null));
                var furniture = _lumina.GetExcelSheet<HousingFurniture>();

                var furnishings = new ItemTreeNode(("Indoor Furnishings", null));
                var tables = new ItemTreeNode(("Tables", null));
                var tabletop = new ItemTreeNode(("Tabletop", null));
                var wallMounted = new ItemTreeNode(("Wall-mounted", null));
                var rugs = new ItemTreeNode(("Rugs", null));

                foreach (var f in furniture)
                {
                    var item = new IndoorFurniture(f);
                    if (String.IsNullOrWhiteSpace(item.Name)) continue;
                    switch (f.HousingItemCategory)
                    {
                        case 12:
                            furnishings.AddChild(item);
                            break;
                        case 13:
                            tables.AddChild(item);
                            break;
                        case 14:
                            tabletop.AddChild(item);
                            break;
                        case 15:
                            wallMounted.AddChild(item);
                            break;
                        case 16:
                            rugs.AddChild(item);
                            break;
                        default:
                            Log.Debug($"Could not get category for {item.Name}");
                            break;
                    }
                }
                ret.AddChild(furnishings);
                ret.AddChild(tables);
                ret.AddChild(tabletop);
                ret.AddChild(wallMounted);
                ret.AddChild(rugs);
                return ret;
            }
            catch (Exception ex)
            {
                Log.Debug($"Could not add housing furniture.");
                Log.Error(ex.Message);
            }
            return null;
        }

        public List<IGear>? GetSharedModels(IGear gear)
        {
            if (_gearModelSets.ContainsKey(gear.Code))
            {
                return _gearModelSets[gear.Code].GetSlot(gear.Slot);
            }
            return new List<IGear>() { gear };
        }

        private int GetPart(EquipmentSlot slot)
        {
            // TODO: I have no idea if the accessories are even like this...
            switch (slot)
            {
                case EquipmentSlot.Head:
                case EquipmentSlot.Ears:
                    return 0;
                case EquipmentSlot.Body:
                case EquipmentSlot.Neck:
                    return 1;
                case EquipmentSlot.Hands:
                case EquipmentSlot.Wrists:
                    return 2;
                case EquipmentSlot.Legs:
                case EquipmentSlot.RightRing:
                    return 3;
                case EquipmentSlot.LeftRing:
                case EquipmentSlot.Feet: return 4;

                default: return 0;
            }
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
                pred = i => i.IsMatch(str);
            }
            var ret = Search(_root, pred);
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
    }
}