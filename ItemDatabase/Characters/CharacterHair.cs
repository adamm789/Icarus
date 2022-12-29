using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.General.Enums;

namespace ItemDatabase.Characters
{
    public class CharacterHair : Character
    {
        public CharacterHair(XivRace race, int num) : base(race, num)
        {
            Name = $"{race.GetDisplayName()} Hair {_num}";
        }

        public override string GetMdlPath()
        {
            return $"chara/human/c{_raceString}/obj/hair/h{_numString}/model/c{_raceString}h{_numString}_hir.mdl";
        }
    }
}
