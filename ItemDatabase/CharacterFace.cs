using ItemDatabase.Enums;
using ItemDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.General.Enums;

namespace ItemDatabase
{
    public class CharacterFace : IItem
    {
        string _raceString;
        string _numString;

        XivRace _race;
        int _num;
        public CharacterFace(XivRace race, int num)
        {
            _race = race;
            _num = num;

            _raceString = race.GetRaceCode();
            _numString = num.ToString().PadLeft(4, '0');

            Name = $"{_race} Face {_num}";

        }

        public string Name { get; }

        public ulong ModelMain => throw new NotImplementedException();

        public MainItemCategory Category => MainItemCategory.Character;

        public string GetMdlFileName()
        {
            return Path.GetFileNameWithoutExtension(GetMdlPath());
        }

        public string GetMdlPath()
        {
            return $"chara/human/c{_raceString}/obj/face/f{_numString}/model/c{_raceString}f{_numString}_fac.mdl";
        }

        public string GetMetadataPath()
        {
            return "";
        }

        public bool IsMatch(string str)
        {
            throw new NotImplementedException();
        }
    }
}
