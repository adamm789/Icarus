using ItemDatabase.Enums;
using System.ComponentModel;

namespace ItemDatabase
{
    public static class XivAttributes
    {
        // https://docs.google.com/spreadsheets/d/1kIKvVsW3fOnVeTi9iZlBDqJo6GWVn6K6BCUIRldEjhw/edit#gid=1898269590
        public static string? GetStringFromType(this XivAttribute attr)
        {
            try
            {
                var enumType = typeof(XivAttribute);
                var memberInfos = enumType.GetMember(attr.ToString());
                var enumValueMemberInfo = memberInfos.First(m => m.DeclaringType == enumType);
                var valueAttributes =
                enumValueMemberInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
                var description = ((DescriptionAttribute)valueAttributes[0]).Description;
                return description;
            }
            catch
            {
                return XivAttribute.Null.ToString();
            }
        }

        public static XivAttribute GetAttributeFromString(string str)
        {
            
            
            if (StringToAttributeDictionary.ContainsKey(str))
            {
                return StringToAttributeDictionary[str];
            }

            return XivAttribute.Null;
            
        }

        public static string GetStringFromAttribute(XivAttribute attr)
        {

            if (!AttributeToStringDictionary.ContainsKey(attr))
            {
                return "Unknown";
            }
            return AttributeToStringDictionary[attr];
        }

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
    }
}
