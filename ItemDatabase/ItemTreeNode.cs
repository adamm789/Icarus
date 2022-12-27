using ItemDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemDatabase
{
    public class ItemTreeNode : ITreeNode<(string, IItem?)>
    {
        public (string, IItem?) Value { get; set; }
        public ICollection<ITreeNode<(string, IItem?)>> Children { get; set; }

        public ItemTreeNode((string, IItem?) value)
        {
            Value = value;
            Children = new List<ITreeNode<(string, IItem?)>>();
        }

        public int CompareTo(object obj)
        {
            if (obj is ITreeNode<(string, IItem?)> t)
            {
                return Value.Item1.CompareTo(t.Value.Item1);
            }
            throw new ArgumentException();
        }
    }
}
