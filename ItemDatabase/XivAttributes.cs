using ItemDatabase.Enums;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace ItemDatabase
{
    public static class XivAttributes
    {
        // https://docs.google.com/spreadsheets/d/1kIKvVsW3fOnVeTi9iZlBDqJo6GWVn6K6BCUIRldEjhw/edit#gid=1898269590
        public static string GetDescriptionFromAttribute(this XivAttribute attr)
        {
            try
            {
                var enumType = typeof(XivAttribute);
                var memberInfos = enumType.GetMember(attr.ToString());
                var enumValueMemberInfo = memberInfos.First(m => m.DeclaringType == enumType);
                var valueAttributes = enumValueMemberInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (valueAttributes.Length > 0)
                {
                    var description = ((DescriptionAttribute)valueAttributes[0]).Description;
                    return description;
                }
            }
            catch
            {

            }
            return "Unknown";

        }

        private static Regex _variantRegex = new(@"{variant}");

        public static string GetVariantAttribute(this XivAttribute attr, string variant)
        {
            var description = attr.GetDescriptionFromAttribute();
            if (!String.IsNullOrWhiteSpace(description))
            {
                return _variantRegex.Replace(description, variant);
            }
            else
            {
                return "Unknown";
            }
        }

        public static bool IsVariantAttribute(this XivAttribute attr)
        {
            return _variantRegex.IsMatch(attr.GetDescriptionFromAttribute());
        }

        private static Dictionary<string, XivAttribute> _stringToAttributeDict = new();
        private static Regex _variantStringRegex = new(@"_[a-j]$");

        public static XivAttribute GetAttributeFromString(string str)
        {
            str = _variantStringRegex.Replace(str, "_{variant}");
            if (_stringToAttributeDict.ContainsKey(str))
            {
                return _stringToAttributeDict[str];
            }
            try
            {
                var enumType = typeof(XivAttribute);
                var enumValues = enumType.GetEnumValues().Cast<XivAttribute>();
                foreach (var e in enumValues)
                {
                    var description = e.GetDescriptionFromAttribute();
                    _stringToAttributeDict.TryAdd(description, e);
                    if (description == str)
                    {
                        return e;
                    }
                }
            }
            catch
            {
            }
            return XivAttribute.Null;

            /*
            if (StringToAttributeDictionary.ContainsKey(str))
            {
                return StringToAttributeDictionary[str];
            }

            return XivAttribute.Null;
            */
        }

        public static string GetStringFromAttribute(this XivAttribute attr)
        {
            /*
            if (!AttributeToStringDictionary.ContainsKey(attr))
            {
                return "Unknown";
            }
            return AttributeToStringDictionary[attr];
            */
            return attr.GetDescriptionFromAttribute();
        }

        /*
        private static Dictionary<string, XivAttribute> StringToAttributeDictionary => AttributeToStringDictionary.ToDictionary(x => x.Value, x => x.Key);

        private static Dictionary<XivAttribute, string> AttributeToStringDictionary = new()
        {
            { XivAttribute.Null, "???" },
            { XivAttribute.Gorget, "atr_inr" },
            { XivAttribute.Wrist, "atr_hij" },
            { XivAttribute.Neck, "atr_nek" },
            { XivAttribute.Elbow, "atr_ude" },
            { XivAttribute.Glove, "atr_arm" },
            { XivAttribute.Knee, "atr_hiz" },
            { XivAttribute.Waist, "atr_kod" },
            { XivAttribute.Shin, "atr_sne" },
            { XivAttribute.Boot, "atr_leg" },
            { XivAttribute.KneePad, "atr_lpd" },

            { XivAttribute.FaceVariantParts, "atr_fv_x" },
            { XivAttribute.FacialHair, "atr_hig" },
            { XivAttribute.Horns, "atr_hrn" },
            { XivAttribute.Face, "atr_kao" },
            { XivAttribute.Ears, "atr_mim" },
            { XivAttribute.HairVariantParts, "atr_hv_x" },
            { XivAttribute.Scalp, "atr_kam" }
        };
        */
    }
}
