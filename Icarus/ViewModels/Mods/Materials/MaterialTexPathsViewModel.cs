using Icarus.Mods;
using Icarus.ViewModels.Util;

namespace Icarus.ViewModels.Mods.Materials
{
    public class MaterialTexPathsViewModel : NotifyPropertyChanged
    {
        MaterialMod _materialMod;
        public MaterialTexPathsViewModel(MaterialModViewModel parent)
        {
        }

        // TODO: Look at TexTools tokenized texture paths
        public string NormalTexPath
        {
            get { return _materialMod.NormalTexPath; }
            set { _materialMod.NormalTexPath = value; OnPropertyChanged(); }
        }

        public string MultiTexPath
        {
            get { return _materialMod.MultiTexPath; }
            set { _materialMod.MultiTexPath = value; OnPropertyChanged(); }
        }

        public string SpecularTexPath
        {
            get { return _materialMod.SpecularTexPath; }
            set { _materialMod.SpecularTexPath = value; OnPropertyChanged(); }
        }

        public string DiffuseTexPath
        {
            get { return _materialMod.DiffuseTexPath; }
            set { _materialMod.DiffuseTexPath = value; OnPropertyChanged(); }
        }

        public string ReflectionTexPath
        {
            get { return _materialMod.ReflectionTexPath; }
            set { _materialMod.ReflectionTexPath = value; OnPropertyChanged(); }
        }
    }
}
