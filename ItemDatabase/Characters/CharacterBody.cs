using ItemDatabase.Enums;
using Lumina;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.General.Enums;

namespace ItemDatabase.Characters
{
    public class CharacterBody : Character
    {
        EquipmentSlot _slot;
        public CharacterBody(XivRace race, int num) : base(race, num)
        {
            Name = $"{race.GetDisplayName()} Body {num}";
            
            if (num == 1)
            {
                _slot = EquipmentSlot.Hands;
            }
            else
            {
                _slot = EquipmentSlot.Body;
            }

        }

        public override string GetMdlPath()
        {
            return $"chara/human/c{_raceString}/obj/body/b{_numString}/model/c{_raceString}b{_numString}_{_slot.GetShortHandSlot()}.mdl";
        }
    }
}
