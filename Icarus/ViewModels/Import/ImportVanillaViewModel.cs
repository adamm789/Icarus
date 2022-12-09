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
            ImportVanillaModelViewModel = new(modPack, itemListViewModel, vanillaFileService.ModelFileService, logService);
            ImportVanillaMaterialViewModel = new(modPack, itemListViewModel, vanillaFileService.MaterialFileService, logService);
            ImportVanillaMetadataViewModel = new(modPack, itemListViewModel, vanillaFileService.MetadataFileService, logService);
            ImportVanillaTextureViewModel = new(modPack, itemListViewModel, vanillaFileService.TextureFileService, logService);

            _itemListViewModel = itemListViewModel;
            _itemListViewModel.PropertyChanged += new(OnItemListPropertyChanged);

            ImportVanillaMaterialViewModel.PropertyChanged += new(OnMaterialChanged);
        }

        private async void OnItemListPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ItemListViewModel.SelectedItem))
            {
                var item = _itemListViewModel.SelectedItem;
                var modelFiles = ImportVanillaModelViewModel.SetItem(item);
                var materials = await ImportVanillaMaterialViewModel.SetItem(item);
                await ImportVanillaMetadataViewModel.SetItem(item);
            }
            else if (e.PropertyName == nameof(ItemListViewModel.CompletePath))
            {
                var completePath = _itemListViewModel.CompletePath;
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
