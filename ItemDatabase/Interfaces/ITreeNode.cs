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
        ICollection<ITreeNode<T>> Children { get; set; }
    }
}
