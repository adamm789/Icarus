using Icarus.ViewModels.Util;

namespace Icarus.ViewModels.Mods.Materials
{
    public class MaterialTexViewModel : NotifyPropertyChanged
    {
        public MaterialTexViewModel(string path)
        {
            Path = path;
        }

        string _path;
        public string Path
        {
            get { return _path; }
            set { _path = value; OnPropertyChanged(); }
        }

        bool _exists;
        public bool Exists
        {
            get { return _exists; }
            set { _exists = value; OnPropertyChanged(); }
        }
    }
}
