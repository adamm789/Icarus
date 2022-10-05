using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.UI
{
    public class BooleanToStringConverter : BooleanConverter<string>
    {
        public BooleanToStringConverter() :
            base("True", "False")
        { }
    }
}
