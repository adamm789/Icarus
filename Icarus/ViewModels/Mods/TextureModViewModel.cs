using Icarus.Mods;
using Icarus.Mods.Interfaces;
using Icarus.Services.GameFiles;
using Icarus.Services.Interfaces;
using ItemDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.Textures.Enums;

namespace Icarus.ViewModels.Mods
{
    public class TextureModViewModel : ModViewModel
    {
        TextureMod _textureMod;
        public TextureModViewModel(TextureMod mod, ItemListService itemListService, IGameFileService gameFileDataService) : base(mod, itemListService, gameFileDataService)
        {
            _textureMod = mod;
        }

        public override string DestinationPath
        {
            get => base.DestinationPath;
            set { base.DestinationPath = value; SetCanExport(); }
        }

        public XivTexType TexType
        {
            get { return _textureMod.TexType; }
            set { _textureMod.TexType = value; OnPropertyChanged(); }
        }

        public override Task SetDestinationItem(IItem? itemArg = null)
        {
            var item = itemArg;
            if (item == null)
            {
                item = _itemListService.SelectedItem;
            }
            if (item == null)
            {
                return Task.CompletedTask;
            }
            _gameFileService.GetTextureData(itemArg);
            DestinationPath = item.GetTexPath(XivTexType.Multi);

            return Task.CompletedTask;
            //throw new NotImplementedException();
        }

        public override bool TrySetDestinationPath(string item)
        {
            //DestinationPath = item;
            return true;
        }
    }
}
