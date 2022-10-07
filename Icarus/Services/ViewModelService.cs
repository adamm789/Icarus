using Icarus.Mods;
using Icarus.Mods.DataContainers;
using Icarus.Mods.Interfaces;
using Icarus.Services.GameFiles.Interfaces;
using Icarus.Services.GameFiles;
using Icarus.Services.Interfaces;
using Icarus.Services.UI;
using Icarus.ViewModels.Models;
using Icarus.ViewModels.Mods;
using ItemDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.General.Enums;
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

        public ModViewModel GetModViewModel(IGameFile mod)
        {
            if (mod is ModelMod modelMod)
            {
                var vm = new ModelModViewModel(modelMod, this, _itemListService, _gameFileService);
                return vm;
            }
            else if (mod is MaterialMod mtrlMod)
            {
                var vm = new MaterialModViewModel(mtrlMod, _itemListService, _gameFileService, _windowService);
                return vm;
            }
            else if (mod is TextureMod texMod)
            {
                var vm = new TextureModViewModel(texMod, _itemListService, _gameFileService);
                return vm;
            }
            else if (mod is ReadOnlyMod readOnlyMod)
            {
                var vm = new ReadOnlyModViewModel(readOnlyMod, _itemListService, _gameFileService);
                return vm;
            }
            else
            {
                return new ReadOnlyModViewModel((IMod)mod, _itemListService, _gameFileService);
            }
        }

        public MeshGroupViewModel GetMeshGroupViewModel(TTMeshGroup meshGroup, ModelModViewModel modelMod)
        {
            var meshGroupViewModel = new MeshGroupViewModel(meshGroup, modelMod, _windowService, _userPreferencesService);
            return meshGroupViewModel;
        }
    }
}
