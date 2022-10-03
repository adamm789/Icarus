using ItemDatabase.Enums;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public override string GetMdlPath()
        {
            // TODO: What about, e.g. Steel Locker which has an "a" appended at the end for the part
            // Is there a way to figure out what the suffixes are?

            return "bgcommon/hou/indoor/general/" + code + "/bgparts/fun_b0_m" + code + ".mdl";
        }

        public override string GetMtrlFileName()
        {
            throw new NotImplementedException();
        }

        public override string GetMtrlPath()
        {
            return GetMtrlPath();
        }

        public string GetMtrlPath(string variant = "0a")
        {
            // bgcommon/hou/indoor/general/1142/material/fun_b0_m1142_0b.mtrl
            return "bgcommon/hou/indoor/general/" + code +"/material/fun_b0_m" + code + "_" + variant + ".mtrl";
        }

        public override string GetTexPath(XivTexType type)
        {
            throw new NotImplementedException();
        }

        public string GetTextPath(string variant = "0a", XivTexType texture = XivTexType.Diffuse)
        {
            return "bgcommon/hou/indoor/general/" + code + "/texture/fun_b0_m" + code + "_" + variant + "_" + _textureDict[texture] + ".tex";
        }
    }
}
