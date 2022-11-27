using Icarus.ViewModels.Util;

namespace Icarus.ViewModels.Models
{
    public class ShapeViewModel : ChildRemovableViewModel
    {
        public ShapeViewModel(string str)
        {
            Name = str;
        }

        string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged(); }
        }
    }
}
