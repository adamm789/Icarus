using xivModdingFramework.Models.DataContainers;
using xivModdingFramework.Models.Helpers;

namespace Icarus.Old
{
    public class ModelMod : Mod
    {
        public TTModel Imported;
        public TTModel Original;
        public XivMdl OgMdl;
        public ModelModifierOptions Options;

        public ModelMod()
        {

        }

        public ModelMod(string name, string filePath, string destinationPath, TTModel imported, TTModel original, XivMdl ogMdl, ModelModifierOptions options)
        {
            FilePath = filePath;
            DestinationPath = destinationPath;

            Imported = imported;
            Original = original;
            OgMdl = ogMdl;
            Options = options;

            DestinationName = name;

            Category = SetCategory(DestinationPath);
        }

        public string SetCategory(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return "";
            }
            str = str.ToLower();
            if (str.Contains("_met."))
            {
                return "Head";
            }
            else if (str.Contains("_top."))
            {
                return "Body";
            }
            else if (str.Contains("_glv."))
            {
                return "Hands";
            }
            else if (str.Contains("_dwn"))
            {
                return "Legs";
            }
            else if (str.Contains("_sho."))
            {
                return "Feet";
            }
            else if (str.Contains("_ear."))
            {
                return "Earring";
            }
            else if (str.Contains("_neck."))
            {
                return "Neck";
            }
            else if (str.Contains("_wrs."))
            {
                return "Wrists";
            }
            else if (str.Contains("_rir") || str.Contains("_ril"))
            {
                return "Rings";
            }
            return "";
        }
    }
}
