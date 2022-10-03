using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.Util.Extensions
{
    public static class ListExtensions
    {
        public static void Move<T>(this IList<T> values, int source, int target)
        {
            var obj = values.ElementAt(source);
            values.RemoveAt(source);
            if (target > values.Count)
            {
                target = values.Count - 1;
            }
            values.Insert(target, obj);
        }
    }
}
