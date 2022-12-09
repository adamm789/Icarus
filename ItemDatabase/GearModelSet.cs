using ItemDatabase.Enums;
using ItemDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemDatabase
{
    public class GearModelSet : IComparable
    {
        public ushort Variant { get; set; }
        Dictionary<EquipmentSlot, List<IGear>> _dict = new();

        public GearModelSet(ushort variant)
        {
            Variant = variant;
        }

        public void Add(IGear gear)
        {
            var slot = gear.Slot;
            if (!_dict.ContainsKey(slot))
            {
                _dict.Add(slot, new List<IGear>());
            }
            _dict[slot].Add(gear);
        }

        public List<IGear>? GetSlot(EquipmentSlot slot)
        {
            if (_dict.ContainsKey(slot))
            {
                return _dict[slot];
            }
            return null;
        }

        public int CompareTo(object? obj)
        {
            if (obj is GearModelSet other)
            {
                return this.Variant - other.Variant;
            }
            throw new ArgumentException("Object is not of type IGearModelSet");
        }
    }
}
