﻿using ItemDatabase.Enums;
using ItemDatabase.Paths;
using Lumina.Data.Files;
using SharpDX.Direct2D1.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.Models.DataContainers;

namespace ItemDatabase
{
    // TODO: Include "Face, Hair"
    public static class AttributePreset
    {
        #region Dictionaries
        // Male Midlander-based body
        static Dictionary<int, List<XivAttribute>> _maleBody;
        public static Dictionary<int, List<XivAttribute>> MaleBody
        {
            get
            {
                return _maleBody ??= _maleBody = new()
                {
                    [1] = new() { XivAttribute.Neck },
                    [2] = new() { XivAttribute.Wrist },
                    [3] = new() { XivAttribute.Elbow },
                };
            }
        }

        // Female Midlander-based body
        static Dictionary<int, List<XivAttribute>> _femaleBody;
        public static Dictionary<int, List<XivAttribute>> FemaleBody
        {
            get
            {
                return _femaleBody ??= _femaleBody = new()
                {
                    [1] = new() { XivAttribute.Wrist },
                    [2] = new() { XivAttribute.Neck },
                    [3] = new() { XivAttribute.Elbow }
                };
            }
        }
        // Male Roe/Hroth, male/female lalafell
        static Dictionary<int, List<XivAttribute>> _maleRoeBody;
        public static Dictionary<int, List<XivAttribute>> MaleRoeBody
        {
            get
            {
                return _maleRoeBody ??= _maleRoeBody = new()
                {
                    [1] = new() { XivAttribute.Elbow },
                    [2] = new() { XivAttribute.Wrist },
                    [3] = new() { XivAttribute.Neck }
                };
            }
        }

        static Dictionary<int, List<XivAttribute>> _legs;
        public static Dictionary<int, List<XivAttribute>> Legs
        {
            get
            {
                return _legs ??= _legs = new()
                {
                    [1] = new() { XivAttribute.Shin },
                    [2] = new() { XivAttribute.Knee }
                };
            }
        }

        static Dictionary<int, List<XivAttribute>> _maleRoeLegs;
        public static Dictionary<int, List<XivAttribute>> MaleRoeLegs
        {
            get
            {
                return _maleRoeLegs ??= _maleRoeLegs = new()
                {
                    [1] = new() { XivAttribute.Knee },
                    [2] = new() { XivAttribute.Shin }

                    // for roe with hr3
                    // Non-Tail Races Only - atr_tkh
                    // Tail Races Only - atr_tls
                };
            }
        }

        static Dictionary<EquipmentSlot, List<XivAttribute>> _standardAttributes;
        public static Dictionary<EquipmentSlot, List<XivAttribute>> StandardAttributes
        {
            get
            {
                return _standardAttributes ??= _standardAttributes = new()
                {
                    [EquipmentSlot.Body] = new() { XivAttribute.Wrist, XivAttribute.Neck, XivAttribute.Elbow },
                    [EquipmentSlot.Legs] = new() { XivAttribute.Knee, XivAttribute.Waist, XivAttribute.Shin }
                };
            }
        }


        static List<XivAttribute> _hairAttributes;
        public static List<XivAttribute> HairAttributes
        {
            get
            {
                return _hairAttributes ??= new() {
                    XivAttribute.HairVariantParts,
                    XivAttribute.Scalp
                };
            }
        }

        static List<XivAttribute> _faceAttributes;
        public static List<XivAttribute> FaceAttributes
        {
            get
            {
                return _faceAttributes ??= new List<XivAttribute>() {
                    XivAttribute.FaceVariantParts,
                    XivAttribute.FacialHair,
                    XivAttribute.Horns,
                    XivAttribute.Face,
                    XivAttribute.Ears
                };
            }
        }

        #endregion

        public static List<XivAttribute>? GetSlotAttributes(EquipmentSlot slot)
        {
            var standard = AttributePreset.StandardAttributes;
            if (standard.ContainsKey(slot))
            {
                return standard[slot];
            }
            return null;
        }

        public static List<XivAttribute>? GetAttributes(string path)
        {
            // TODO: Get attributes via string (_met, _sho, etc)
            if (String.IsNullOrWhiteSpace(path)) return null;
            var retVal = new List<XivAttribute>();
            path = path.ToLower();
            var slot = XivPathParser.GetEquipmentSlot(path);
            var slotAttributes = GetSlotAttributes(slot);
            if (slotAttributes != null)
            {
                retVal.AddRange(slotAttributes);
            }

            if (XivPathParser.IsFace(path))
            {
                retVal.AddRange(FaceAttributes);
            }
            else if (XivPathParser.IsHair(path))
            {
                retVal.AddRange(HairAttributes);
            }
            // TODO: Other body part attributes

            return retVal;
        }

        public static Dictionary<string, Dictionary<int, List<XivAttribute>>> GetAttributeBodyPresets(EquipmentSlot slot)
        {
            var dict = new Dictionary<string, Dictionary<int, List<XivAttribute>>>();
            switch (slot)
            {
                case EquipmentSlot.Body:
                    dict.Add("Male Midlander Body", AttributePreset.MaleBody);
                    dict.Add("Female Body", AttributePreset.FemaleBody);
                    dict.Add("Male Roe/Lalafell Body", AttributePreset.MaleRoeBody);
                    break;
                case EquipmentSlot.Legs:
                    dict.Add("Legs", AttributePreset.Legs);
                    dict.Add("Male Roe Legs", AttributePreset.MaleRoeLegs);
                    break;
            }
            return dict;
        }
        public static Dictionary<string, Dictionary<int, List<XivAttribute>>> GetAttributeTTModelPresets(TTModel ttModel)
        {
            var dict = new Dictionary<string, Dictionary<int, List<XivAttribute>>>();

            // Get the attributes from the given TTModel

            for (var i = 0; i < ttModel.MeshGroups.Count; i++)
            {
                var group = ttModel.MeshGroups[i];
                var partDict = new Dictionary<int, List<XivAttribute>>();
                for (var j = 0; j < group.Parts.Count; j++)
                {
                    var part = group.Parts[j];
                    foreach (var attr in part.Attributes)
                    {
                        var xivAttribute = XivAttributes.GetAttributeFromString(attr);
                        if (xivAttribute != XivAttribute.Null)
                        {
                            if (!partDict.ContainsKey(j))
                            {
                                partDict[j] = new();
                            }
                            partDict[j].Add(xivAttribute);
                        }
                    }
                }
                if (partDict.Keys.Count > 0)
                {
                    var groupName = $"Group {i} ";
                    dict.Add(groupName, partDict);
                }
            }
            return dict;
        }
    }
}