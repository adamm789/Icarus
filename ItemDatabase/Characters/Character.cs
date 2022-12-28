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
    public abstract class Character : IItem, IComparable
    {
        protected string _raceString;
        protected string _numString;

        protected XivRace _race;
        public int _num;
        public Character(XivRace race, int num)
        {
            _race = race;
            _num = num;

            _raceString = race.GetRaceCode();
            _numString = num.ToString().PadLeft(4, '0');
        }

        public string Name { get; set; }

        public ulong ModelMain => throw new NotImplementedException();

        public MainItemCategory Category => MainItemCategory.Character;

        public string GetMdlFileName()
        {
            return Path.GetFileNameWithoutExtension(GetMdlPath());
        }

        public abstract string GetMdlPath();

        public string GetMetadataPath()
        {
            return "";
        }

        public bool IsMatch(string str)
        {
            return Name.Contains(str);
        }

        public int CompareTo(object? obj)
        {
            if (obj is Character c)
            {
                return _num.CompareTo(c._num);
            }
            throw new ArgumentException();
        }
    }
}
