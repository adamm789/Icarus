using Icarus.Mods;
using Icarus.Mods.DataContainers;
using Icarus.Mods.Interfaces;
using Icarus.Services.Files;
using Icarus.Services.GameFiles;
using Icarus.Services.GameFiles.Interfaces;
using Icarus.Services.Interfaces;
using Icarus.Services.UI;
using Icarus.ViewModels.Export;
using Icarus.ViewModels.Import;
using Icarus.ViewModels.Items;
using Icarus.ViewModels.Models;
using Icarus.ViewModels.Mods;
using Icarus.ViewModels.Mods.DataContainers;
using Icarus.ViewModels.Mods.DataContainers.Interfaces;
using xivModdingFramework.Models.DataContainers;

namespace Icarus.Services
{
    public class ViewModelService : ServiceBase<ViewModelService>
    {
        public IModPackViewModel ModPackViewModel { get; private set; }

        readonly IGameFileService _gameFileService;
        readonly IUserPreferencesService _userPreferencesService;
        readonly IWindowService _windowService;
        readonly ILogService _logService;

        readonly IModelFileService _modelFileService;
        readonly IMaterialFileService _materialFileService;
        readonly ITextureFileService _textureFileService;
        readonly IMetadataFileService _metadataFileService;

        public ViewModelService(IGameFileService gameFileDataService, IUserPreferencesService userPreferencesService, IWindowService windowService, ILogService logService, VanillaFileService vanillaFileService)
        {
            _gameFileService = gameFileDataService;
            _userPreferencesService = userPreferencesService;
            _windowService = windowService;
            _logService = logService;

            _modelFileService = vanillaFileService.ModelFileService;
            _materialFileService = vanillaFileService.MaterialFileService;
            _textureFileService = vanillaFileService.TextureFileService;
            _metadataFileService = vanillaFileService.MetadataFileService;
        }

        public IModsListViewModel GetModsListViewModel(ModPack modPack)
        {
            return new ModsListViewModel(modPack, this, _windowService, _logService);
        }

        public ModPackMetaViewModel GetModPackMetaViewModel(ModPack modPack, bool isReadOnly = false)
        {
            return new ModPackMetaViewModel(modPack, _userPreferencesService, _logService, isReadOnly);
        }

        public ModViewModel GetModViewModel(IMod file)
        {
            if (file is ModelMod modelMod)
            {
                var vm = new ModelModViewModel(modelMod, this, _modelFileService, _logService);
                return vm;
            }
            else if (file is MaterialMod mtrlMod)
            {
                var vm = new MaterialModViewModel(mtrlMod, _materialFileService, _windowService, _logService);
                return vm;
            }
            else if (file is TextureMod texMod)
            {
                var vm = new TextureModViewModel(texMod, _textureFileService, _logService);
                return vm;
            }
            else if (file is MetadataMod metaMod)
            {
                var vm = new MetadataModViewModel(metaMod, _metadataFileService, _windowService, _logService);
                return vm;
            }
            else
            {
                _logService.Warning($"Fell through to \"ReadOnlyMod\".");
                return new ReadOnlyModViewModel(file, _logService);
            }
        }

        public MeshGroupViewModel GetMeshGroupViewModel(TTMeshGroup meshGroup, ModelModViewModel modelMod)
        {
            var meshGroupViewModel = new MeshGroupViewModel(meshGroup, modelMod, this, _windowService);
            return meshGroupViewModel;
        }

        public MeshGroupMaterialViewModel GetMeshGroupMaterialViewModel(TTMeshGroup group, ModelModViewModel model)
        {
            var meshGroupMaterialViewModel = new MeshGroupMaterialViewModel(group, model, _userPreferencesService);
            return meshGroupMaterialViewModel;
        }
    }
}
