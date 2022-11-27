using Icarus.ViewModels.Models;
using ItemDatabase;
using ItemDatabase.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Icarus.ViewModels.Models
{
    public class VariantAttributeViewModel : AttributeViewModel
    {
        private XivAttribute _xivAttribute;
        public VariantAttributeViewModel(XivAttribute attr, string variant = "a")
        {
            _xivAttribute = attr;
            SelectedVariant = variant;
            _attributeName = attr.GetVariantAttribute(variant);
            DisplayedName = $"{_attributeName} ({_xivAttribute})";
        }

        public override VariantAttributeViewModel Copy()
        {
            return new VariantAttributeViewModel(this._xivAttribute, this.SelectedVariant);
        }

        string _selectedVariant = "a";
        public string SelectedVariant
        {
            get { return _selectedVariant; }
            set
            {
                _selectedVariant = value;
                _attributeName = _xivAttribute.GetVariantAttribute(value);
                DisplayedName = $"{_attributeName} ({_xivAttribute})";
            }
        }

        public List<string> AttributeVariants { get; } = new()
        {
            "a","b","c","d","e","f","g","h","i","j"
        };
    }
}
