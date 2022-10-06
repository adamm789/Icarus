using Icarus.Mods;
using Icarus.Mods.Interfaces;
using Icarus.Services.GameFiles;
using Icarus.Services.Interfaces;
using ItemDatabase.Interfaces;
using ItemDatabase.Paths;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.Materials.DataContainers;
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

        public XivTexType TexType
        {
            get { return _textureMod.TexType; }
            set {
                _textureMod.TexType = value;
                OnPropertyChanged();
                DestinationPath = XivPathParser.ChangeTexType(DestinationPath, _textureMod.TexType);
            }
        }

        public override void SetModData(IGameFile gameFile)
        {
            if (gameFile is not ITextureGameFile texGameFile)
            {
                return;
            }
            base.SetModData(gameFile);
        }

        public List<XivTexType> TexTypeValues { get; } = Enum.GetValues(typeof(XivTexType)).Cast<XivTexType>().ToList();


        /*
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
            var variant = XivPathParser.GetTexVariant(DestinationPath);
            var path = item.GetTexPath(TexType, variant);

            DestinationPath = path;
            DestinationName = item.Name;

            _textureMod.Name = item.Name;

            if (item is IGear gear)
            {
                _textureMod.Category = gear.Slot.ToString();
            }
            else
            {
                _textureMod.Category = item.Category.ToString();
            }
            return Task.CompletedTask;
        }
        */
    }
}
