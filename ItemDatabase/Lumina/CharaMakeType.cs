using Lumina.Data;
using Lumina.Excel;
using Lumina;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lumina.Excel.GeneratedSheets;

namespace ItemDatabase.Lumina
{
    [Sheet("CharaMakeType", columnHash: 0x80d7db6d)]
    public class CharaMakeType : ExcelRow
    {
        public LazyRow<Race> Race { get; set; }
        public LazyRow<Tribe> Tribe { get; set; }
        public sbyte Gender { get; set; }
        public LazyRow<Lobby>[] Menu { get; set; }
        public byte[] InitVal { get; set; }
        public byte[] SubMenuType { get; set; }
        public byte[] SubMenuNum { get; set; }
        public byte[] LookAt { get; set; }
        public uint[] SubMenuMask { get; set; }
        public uint[] Customize { get; set; }
        public uint[][] SubMenuParam { get; set; }
        public override void PopulateData(RowParser parser, GameData gameData, Language language)
        {
            base.PopulateData(parser, gameData, language);

            Race = new LazyRow<Race>(gameData, parser.ReadColumn<int>(0), language);
            Tribe = new LazyRow<Tribe>(gameData, parser.ReadColumn<int>(1), language);
            Gender = parser.ReadColumn<sbyte>(2);
            Menu = new LazyRow<Lobby>[28];
            for (var i = 0; i < 28; i++)
                Menu[i] = new LazyRow<Lobby>(gameData, parser.ReadColumn<uint>(3 + i), language);
            InitVal = new byte[28];
            for (var i = 0; i < 28; i++)
                InitVal[i] = parser.ReadColumn<byte>(31 + i);
            SubMenuType = new byte[28];
            for (var i = 0; i < 28; i++)
                SubMenuType[i] = parser.ReadColumn<byte>(59 + i);
            SubMenuNum = new byte[28];
            for (var i = 0; i < 28; i++)
                SubMenuNum[i] = parser.ReadColumn<byte>(87 + i);
            LookAt = new byte[28];
            for (var i = 0; i < 28; i++)
                LookAt[i] = parser.ReadColumn<byte>(115 + i);
            SubMenuMask = new uint[28];
            for (var i = 0; i < 28; i++)
                SubMenuMask[i] = parser.ReadColumn<uint>(143 + i);
            Customize = new uint[28];
            for (var i = 0; i < 28; i++)
                Customize[i] = parser.ReadColumn<uint>(171 + i);
            SubMenuParam = new uint[28][];
            for (var i = 0; i < 28; i++)
            {
                SubMenuParam[i] = new uint[100];
                for (var j = 0; j < 100; j++)
                {
                    var num = 199 + i + j * 28;
                    SubMenuParam[i][j] = parser.ReadColumn<uint>(num);
                }
            }
        }
    }

}
