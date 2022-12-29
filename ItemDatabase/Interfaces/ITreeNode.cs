using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemDatabase.Interfaces
{
    public interface ITreeNode<T> : IComparable
    {
        T Value { get; set; }
        IList<ITreeNode<T>> Children { get; set; }
        void AddChild(ITreeNode<T> node);
        void AddChild(T value);
        void AddChildren(IEnumerable<ITreeNode<T>> children);
    }
}
