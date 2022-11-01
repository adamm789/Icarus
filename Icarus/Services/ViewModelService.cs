using Icarus.Mods;
using Icarus.Mods.DataContainers;
using Icarus.Mods.Interfaces;
using Icarus.Services.GameFiles;
using Icarus.Services.GameFiles.Interfaces;
using Icarus.Services.Interfaces;
using Icarus.ViewModels.Models;
using Icarus.ViewModels.Mods;
using Icarus.ViewModels.Mods.DataContainers;
using xivModdingFramework.Models.DataContainers;

namespace Icarus.Services
{
    public class ViewModelService : ServiceBase<ViewModelService>
    {
        readonly IGameFileService _gameFileService;
        readonly ISettingsService _settingsService;
        readonly IUserPreferencesService _userPreferencesService;
        readonly IWindowService _windowService;
        readonly ILogService _logService;

        public ViewModelService(IGameFileService gameFileDataService, ISettingsService settingsService,
            IUserPreferencesService userPreferencesService, IWindowService windowService, ILogService logService)
        {
            _gameFileService = gameFileDataService;
            _userPreferencesService = userPreferencesService;
            _windowService = windowService;
            _settingsService = settingsService;
            _logService = logService;
        }

        public ModPackMetaViewModel GetModPackMetaViewModel(ModPack modPack)
        {
            return new ModPackMetaViewModel(modPack, _userPreferencesService);
        }

        public ModViewModel GetModViewModel(IMod file)
        {
            if (file is ModelMod modelMod)
            {
                var vm = new ModelModViewModel(modelMod, this, _gameFileService, _logService);
                return vm;
            }
            else if (file is MaterialMod mtrlMod)
            {
                var vm = new MaterialModViewModel(mtrlMod, _gameFileService, _windowService, _logService);
                return vm;
            }
            else if (file is TextureMod texMod)
            {
                var vm = new TextureModViewModel(texMod, _gameFileService, _logService);
                return vm;
            }
            else if (file is MetadataMod metaMod)
            {
                var vm = new MetadataModViewModel(metaMod, _gameFileService, _windowService, _logService);
                return vm;
            }
            else if (file is ReadOnlyMod readOnlyMod)
            {
                var vm = new ReadOnlyModViewModel(readOnlyMod, _gameFileService, _logService);
                return vm;
            }
            else
            {
                return new ReadOnlyModViewModel((IMod)file, _gameFileService, _logService);
            }
        }

        public MeshGroupViewModel GetMeshGroupViewModel(TTMeshGroup meshGroup, ModelModViewModel modelMod)
        {
            var meshGroupViewModel = new MeshGroupViewModel(meshGroup, modelMod, _windowService, this);
            return meshGroupViewModel;
        }

        public MeshGroupMaterialViewModel GetMeshGroupMaterialViewModel(TTMeshGroup group, ModelModViewModel model)
        {
            var meshGroupMaterialViewModel = new MeshGroupMaterialViewModel(group, model, _userPreferencesService, _settingsService);
            return meshGroupMaterialViewModel;
        }
    }
}
