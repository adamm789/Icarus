using Icarus.ViewModels.Util;
using ItemDatabase;
using ItemDatabase.Enums;

namespace Icarus.ViewModels.Models
{
    public class AttributeViewModel : ChildRemovableViewModel
    {
        protected XivAttribute _attribute;
        protected string _attributeName;
        public AttributeViewModel()
        {

        }
        public AttributeViewModel(string str)
        {
            _attributeName = str;
            _attribute = XivAttributes.GetAttributeFromString(str);

            var englishName = XivAttributes.GetStringFromAttribute(_attribute);
            DisplayedName = FormatDisplayedName(_attributeName, _attribute.ToString());
        }

        public AttributeViewModel(XivAttribute attr)
        {
            _attribute = attr;

            // TODO: Error checking
            _attributeName = XivAttributes.GetStringFromAttribute(attr);
            DisplayedName = FormatDisplayedName(_attributeName, attr.ToString());
        }

        private string FormatDisplayedName(string atr, string attribute)
        {
            return atr + " (" + attribute.ToLower() + ")";

        }
        string _displayedName = "";
        public string DisplayedName
        {
            get { return _displayedName; }
            protected set { _displayedName = value; OnPropertyChanged(); }
        }

        public XivAttribute GetAttribute()
        {
            return _attribute;
        }

        public string GetAttributeString()
        {
            return _attributeName;
        }
    }
}
