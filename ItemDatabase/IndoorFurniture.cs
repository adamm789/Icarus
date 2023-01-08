using Lumina.Excel.GeneratedSheets;

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

        public override bool IsMatch(string str)
        {
            return base.IsMatch(str) || code.Contains(str, StringComparison.OrdinalIgnoreCase);
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
    }
}
