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
        readonly IUserPreferencesService _userPreferencesService;
        ItemListService _itemListService;
        readonly IWindowService _windowService;

        public ViewModelService(IGameFileService gameFileDataService, ItemListService itemListService, IUserPreferencesService userPreferencesService, IWindowService windowService)
        {
            _gameFileService = gameFileDataService;
            _userPreferencesService = userPreferencesService;
            _windowService = windowService;
            _itemListService = itemListService;
        }
        public ModPackMetaViewModel GetModPackMetaViewModel(ModPack modPack)
        {
            return new ModPackMetaViewModel(modPack, _userPreferencesService);
        }

        public ModViewModel GetModViewModel(IGameFile file)
        {

            if (file is ModelMod modelMod)
            {
                var vm = new ModelModViewModel(modelMod, this, _gameFileService);
                return vm;
            }
            else if (file is MaterialMod mtrlMod)
            {
                var vm = new MaterialModViewModel(mtrlMod, _gameFileService, _windowService);
                return vm;
            }
            else if (file is TextureMod texMod)
            {
                var vm = new TextureModViewModel(texMod, _gameFileService);
                return vm;
            }
            else if (file is ReadOnlyMod readOnlyMod)
            {
                var vm = new ReadOnlyModViewModel(readOnlyMod, _gameFileService);
                return vm;
            }
            else
            {
                return new ReadOnlyModViewModel((IMod)file, _gameFileService);
            }
        }

        public MeshGroupViewModel GetMeshGroupViewModel(TTMeshGroup meshGroup, ModelModViewModel modelMod)
        {
            var meshGroupViewModel = new MeshGroupViewModel(meshGroup, modelMod, _windowService, _userPreferencesService);
            return meshGroupViewModel;
        }
    }
}
