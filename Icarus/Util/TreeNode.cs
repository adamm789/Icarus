using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.Util
{
    public class TreeNode<T>
    {
        readonly T _value;
        readonly IList<T> _children;

        public TreeNode(T value)
        {

        }

        public TreeNode(string header, IList<T> values)
        {
            
        }
    }
}
