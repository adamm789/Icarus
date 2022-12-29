using ItemDatabase.Enums;
using ItemDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.General.Enums;

namespace ItemDatabase.Characters
{
    public class CharacterFace : Character
    {
        public CharacterFace(XivRace race, int num) : base(race, num)
        {
            Name = $"{_race.GetDisplayName()} Face {_num}";
        }
        public override string GetMdlPath()
        {

            return $"chara/human/c{_raceString}/obj/face/f{_numString}/model/c{_raceString}f{_numString}_fac.mdl";
        }
    }
}
