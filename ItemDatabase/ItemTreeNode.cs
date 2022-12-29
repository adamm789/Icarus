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
        public IList<ITreeNode<(string, IItem?)>> Children { get; set; }

        public ItemTreeNode((string, IItem?) value)
        {
            Value = value;
            Children = new List<ITreeNode<(string, IItem?)>>();
        }
        public void AddChild((string, IItem?) value)
        {
            AddChild(new ItemTreeNode(value));
        }

        public void AddChild(ITreeNode<(string, IItem?)> child)
        {
            Children.Add(child);
        }

        public void AddChildren(IEnumerable<ITreeNode<(string, IItem?)>> children)
        {
            foreach (var child in children)
            {
                AddChild(child);
            }
        }

        public void AddChild(string str, IItem? item)
        {
            AddChild((str, item));
        }

        public void AddChild(IItem item)
        {
            AddChild(item.Name, item);
        }

        public int CompareTo(object? obj)
        {
            if (obj is ItemTreeNode node)
            {
                return Value.Item1.CompareTo(node.Value.Item1);
            }
            throw new ArgumentException($"Cannot compare {obj?.GetType()} to {GetType()}");
        }
    }
}
