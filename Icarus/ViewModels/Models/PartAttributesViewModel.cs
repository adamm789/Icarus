using Icarus.ViewModels.Util;
using ItemDatabase.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.ViewModels.Models
{
    public class PartAttributesViewModel : NotifyPropertyChanged
    {
        public PartAttributesViewModel(string header, List<XivAttribute> attributes)
        {
            PartHeader = header;
            Attributes = new();
            foreach (var attr in attributes)
            {
                var vm = new AttributeViewModel(attr);
                Attributes.Add(vm);
            }
        }

        public string PartHeader { get; set; }

        public ObservableCollection<AttributeViewModel> Attributes { get; set; }
    }
}
