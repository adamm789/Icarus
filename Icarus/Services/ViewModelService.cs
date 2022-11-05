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
        readonly ISettingsService _settingsService;
        readonly IUserPreferencesService _userPreferencesService;
        readonly IWindowService _windowService;
        readonly ILogService _logService;

        readonly IItemListService _itemListService;
        readonly IMessageBoxService _messageBoxService;
        readonly ExportService _exportService;
        readonly ImportService _importService;

        public ViewModelService(IGameFileService gameFileDataService, ISettingsService settingsService,
            IUserPreferencesService userPreferencesService, IWindowService windowService, ILogService logService)
        {
            _gameFileService = gameFileDataService;
            _userPreferencesService = userPreferencesService;
            _windowService = windowService;
            _settingsService = settingsService;
            _logService = logService;
        }

        public IModPackViewModel SetModPackViewModel()
        {
            var modPack = new ModPack();
            ModPackViewModel = new ModPackViewModel(modPack, this);
            var modPackListViewModel = new ModPackListViewModel(ModPackViewModel.ModsListViewModel, this);

            var itemListViewModel = new ItemListViewModel(_itemListService, _logService);
            var importVanillaViewModel = new ImportVanillaViewModel(ModPackViewModel.ModsListViewModel, itemListViewModel, _gameFileService, _logService);
            var ImportViewModel = new ImportViewModel(ModPackViewModel, modPackListViewModel, _importService, _settingsService, _logService);

            var exportViewModel = new ExportViewModel(ModPackViewModel.ModsListViewModel, _messageBoxService, _exportService);

            return ModPackViewModel;
        }

        public ModPackMetaViewModel GetModPackMetaViewModel(ModPack modPack, bool isReadOnly = false)
        {
            return new ModPackMetaViewModel(modPack, _userPreferencesService, isReadOnly);
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
            else
            {
                return new ReadOnlyModViewModel(file, _gameFileService, _logService);
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
