using Icarus.Services.GameFiles;
using Icarus.Services.Interfaces;
using Icarus.ViewModels.Items;
using Icarus.ViewModels.Mods.DataContainers.Interfaces;
using Icarus.ViewModels.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Wpf.Toolkit.Primitives;
using xivModdingFramework.Materials.FileTypes;

namespace Icarus.ViewModels.Import
{
    public class ImportVanillaViewModel : ViewModelBase
    {
        public ImportVanillaModelViewModel ImportVanillaModelViewModel { get; set; }
        public ImportVanillaMaterialViewModel ImportVanillaMaterialViewModel { get; set; }
        public ImportVanillaMetadataViewModel ImportVanillaMetadataViewModel { get; set; }
        public ImportVanillaTextureViewModel ImportVanillaTextureViewModel { get; set; }

        readonly ItemListViewModel _itemListViewModel;

        public ImportVanillaViewModel(IModsListViewModel modPack, ItemListViewModel itemListViewModel, VanillaFileService vanillaFileService, ILogService logService)
            : base(logService)
        {
            ImportVanillaModelViewModel = new(modPack, vanillaFileService.ModelFileService, logService);
            ImportVanillaMaterialViewModel = new(modPack, vanillaFileService.MaterialFileService, logService);
            ImportVanillaMetadataViewModel = new(modPack, vanillaFileService.MetadataFileService, logService);
            ImportVanillaTextureViewModel = new(modPack, vanillaFileService.TextureFileService, logService);

            _itemListViewModel = itemListViewModel;
            _itemListViewModel.PropertyChanged += new(OnItemListPropertyChanged);

            ImportVanillaMaterialViewModel.PropertyChanged += new(OnMaterialChanged);
        }

        private async void OnItemListPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ItemListViewModel.SelectedItem))
            {
                var item = _itemListViewModel.SelectedItem;
                await ImportVanillaModelViewModel.SetItem(item);
                await ImportVanillaMaterialViewModel.SetItem(item);
                await ImportVanillaMetadataViewModel.SetItem(item);
            }
            else if (e.PropertyName == nameof(ItemListViewModel.CompletePath))
            {
                if (_itemListViewModel.SelectedItem == null)
                {
                    // chara/monster/m0791/obj/body/b0001/model/m0791b0001.mdl
                    var completePath = _itemListViewModel.CompletePath;

                    await ImportVanillaModelViewModel.SetCompletePath(completePath);

                    if (ImportVanillaModelViewModel.SelectedModelFile != null)
                    {
                        await ImportVanillaMaterialViewModel.SetModel(ImportVanillaModelViewModel.SelectedModelFile);
                    }
                    else
                    {
                        await ImportVanillaMaterialViewModel.SetCompletePath(completePath);
                    }

                    if (ImportVanillaMaterialViewModel.SelectedMaterialFile != null)
                    {
                        ImportVanillaTextureViewModel.SetMaterial(ImportVanillaMaterialViewModel.SelectedMaterialFile);
                    }
                    else
                    {
                        await ImportVanillaTextureViewModel.SetCompletePath(completePath);
                    }
                    await ImportVanillaMetadataViewModel.SetCompletePath(completePath);
                }
            }
        }

        private void OnMaterialChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ImportVanillaMaterialViewModel.SelectedMaterialFile))
            {
                var material = ImportVanillaMaterialViewModel.SelectedMaterialFile;
                ImportVanillaTextureViewModel.SetMaterial(material);
            }
        }
    }
}
