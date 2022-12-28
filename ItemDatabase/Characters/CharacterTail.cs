using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.General.Enums;

namespace ItemDatabase.Characters
{
    public class CharacterTail : Character
    {
        public CharacterTail(XivRace race, int num) : base(race, num)
        {
            Name = $"{_race} Tail {_num}";
        }

        public override string GetMdlPath()
        {
            return $"chara/human/c{_raceString}/obj/tail/t{_numString}/model/c{_raceString}t{_numString}_til.mdl";
        }
    }
}
