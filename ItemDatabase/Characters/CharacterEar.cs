using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.General.Enums;

namespace ItemDatabase.Characters
{
    public class CharacterEar : Character
    {
        public CharacterEar(XivRace race, int num) : base(race, num)
        {
            Name = $"{race.GetDisplayName()} Ear {num}";
        }
        public override string GetMdlPath()
        {
            return $"chara/human/c{_raceString}/obj/zear/z{_numString}/model/c{_raceString}z{_numString}_zer.mdl"; ;
        }
    }
}
