using Icarus.ViewModels.Util;

namespace Icarus.ViewModels.Models
{
    // TODO: Check to see if removing shapes actually changes the model
    public class ShapeViewModel : ChildRemovableViewModel
    {
        // TODO: Include the "english name" for the shapes?
        // e.g. shp_ude (elbow)
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
