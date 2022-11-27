using Icarus.ViewModels.Util;
using ItemDatabase;
using ItemDatabase.Enums;
using System.Collections.Generic;
using System.Windows.Documents;

namespace Icarus.ViewModels.Models
{
    public class AttributeViewModel : ChildRemovableViewModel
    {
        protected string _attributeName;

        public AttributeViewModel()
        {

        }
        public AttributeViewModel(string attr)
        {
            _attributeName = attr;
            var xivAttribute = XivAttributes.GetAttributeFromString(_attributeName);
            DisplayedName = $"{attr} ({xivAttribute})";
        }


        public AttributeViewModel(XivAttribute attr)
        {
            // TODO: Error checking
            _attributeName = XivAttributes.GetStringFromAttribute(attr);
            DisplayedName = $"{_attributeName} ({attr})";
        }

        public virtual AttributeViewModel Copy()
        {
            return new AttributeViewModel()
            {
                _attributeName = this._attributeName,
                DisplayedName = this.DisplayedName
            };
        }

        string _displayedName = "";
        public string DisplayedName
        {
            get { return _displayedName; }
            protected set { _displayedName = value; OnPropertyChanged(); }
        }

        public string GetAttributeString()
        {
            return _attributeName;
        }
    }
}
