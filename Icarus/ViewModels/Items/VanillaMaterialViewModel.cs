using Icarus.Mods.Interfaces;
using Icarus.Services.Interfaces;
using Icarus.ViewModels.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.ViewModels.Items
{
    public class VanillaMaterialViewModel : ViewModelBase
    {
        public VanillaMaterialViewModel(List<IMaterialGameFile>? materials, ILogService logService) : base(logService)
        {
            SetMaterials(materials);
        }

        public void SetMaterials(List<IMaterialGameFile>? materials)
        {
            if (materials == null)
            {
                Path = "";
                Textures = new();
                Names = new();
                return;
            }
            MaterialId = materials.First().MaterialSet;
            //Path = materials.First().Path;
            Path = materials.First().XivMtrl.MTRLPath;
            Textures = materials.First().XivMtrl.TexturePathList;

            Names.Clear();

            foreach (var material in materials)
            {
                Names.Add(material.Name);
            }
        }

        int _materialId = -1;
        public int MaterialId
        {
            get { return _materialId; }
            set { _materialId = value; OnPropertyChanged(); }
        }

        string _path = "";
        public string Path
        {
            get { return _path; }
            set { _path = value; OnPropertyChanged(); }
        }

        List<string> _names = new();
        public List<string> Names
        {
            get { return _names; }
            set { _names = value; OnPropertyChanged(); }
        }
        List<string> _textures = new();
        public List<string> Textures
        {
            get { return _textures; }
            set { _textures = value; OnPropertyChanged(); }
        }
    }
}
