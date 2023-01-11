using Icarus.Mods.Interfaces;
using Icarus.Services.Interfaces;
using Icarus.ViewModels.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.ViewModels.Items
{
    public class VanillaFileViewModel : ViewModelBase
    {
        List<IModelGameFile>? _modelFiles;
        public List<IModelGameFile>? ModelFiles
        {
            get { return _modelFiles; }
            set { _modelFiles = value; OnPropertyChanged(); }
        }

        List<IMaterialGameFile>? _materialFiles;
        public List<IMaterialGameFile>? MaterialFiles
        {
            get { return _materialFiles; }
            set { _materialFiles = value; OnPropertyChanged(); }
        }

        Dictionary<string, List<IMaterialGameFile>>? _materialsDict;
        public Dictionary<string, List<IMaterialGameFile>>? MaterialsDict
        {
            get { return _materialsDict; }
            set
            {
                _materialsDict = value;
                OnPropertyChanged();
                if (value != null)
                {
                    Materials = new();
                    foreach (var kvp in value)
                    {
                        Materials.Add(new VanillaMaterialViewModel(kvp.Value, _logService));
                    }
                }
                else
                {
                    Materials = null;
                }
            }
        }

        List<ITextureGameFile>? _textureFiles;
        public List<ITextureGameFile>? TextureFiles
        {
            get { return _textureFiles; }
            set { _textureFiles = value; OnPropertyChanged(); }
        }

        IMetadataFile? _metadataFile;
        public IMetadataFile? MetadataFile
        {
            get { return _metadataFile; }
            set { _metadataFile = value; OnPropertyChanged(); }
        }

        ObservableCollection<VanillaMaterialViewModel>? _materials = new();
        public ObservableCollection<VanillaMaterialViewModel>? Materials
        {
            get { return _materials; }
            set { _materials = value; OnPropertyChanged(); }
        }

        public VanillaFileViewModel(ILogService logService)
            : base(logService)
        {
            
        }
    }
}
