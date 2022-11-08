using Lumina.Excel.GeneratedSheets;
using xivModdingFramework.Textures.Enums;

namespace ItemDatabase
{
    public class IndoorFurniture : Item
    {
        string code;
        public IndoorFurniture(HousingFurniture furniture)
        {
            _item = furniture.Item.Value;
            Name = _item.Name;
            ModelMain = furniture.ModelKey;
            code = ModelMain.ToString().PadLeft(4, '0');

            if (Name == "Steel Locker")
            {
                var str = GetMdlPath();
            }
        }

        public override string GetMdlFileName()
        {
            //throw new NotImplementedException();
            return "/fun_b0_m" + code + ".mdl";
        }

        public override string GetMdlPath()
        {
            // TODO: What about, e.g. Steel Locker which has an "a" appended at the end for the part
            // Is there a way to figure out what the suffixes are?

            return "bgcommon/hou/indoor/general/" + code + "/bgparts/fun_b0_m" + code + ".mdl";
        }

        public override string GetMtrlFileName()
        {
            //throw new NotImplementedException();
            return GetMtrlFileName("0a");
        }

        public string GetMtrlFileName(string variant)
        {
            return "fun_b0_m" + code + "_" + variant + ".mtrl";
        }

        public override string GetMtrlPath(string variant = "0a")
        {
            // bgcommon/hou/indoor/general/1142/material/fun_b0_m1142_0b.mtrl
            return "bgcommon/hou/indoor/general/" + code + "/material/fun_b0_m" + code + "_" + variant + ".mtrl";
        }

        public override string GetTexPath(XivTexType type, string variant = "")
        {
            throw new NotImplementedException();
        }

        public string GetTexPath(string variant = "0a", XivTexType texture = XivTexType.Diffuse)
        {
            return "bgcommon/hou/indoor/general/" + code + "/texture/fun_b0_m" + code + "_" + variant + "_" + _textureDict[texture] + ".tex";
        }
    }
}
