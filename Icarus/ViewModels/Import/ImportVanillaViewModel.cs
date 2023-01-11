using Icarus.Mods.Interfaces;
using Icarus.Services.GameFiles;
using Icarus.Services.Interfaces;
using Icarus.ViewModels.Items;
using Icarus.ViewModels.Mods.DataContainers.Interfaces;
using Icarus.ViewModels.Util;
using Icarus.Views.Items;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Icarus.ViewModels.Import
{
    public class ImportVanillaViewModel : ViewModelBase
    {
        public ImportVanillaModelViewModel ImportVanillaModelViewModel { get; set; }
        public ImportVanillaMaterialViewModel ImportVanillaMaterialViewModel { get; set; }
        public ImportVanillaMetadataViewModel ImportVanillaMetadataViewModel { get; set; }
        public ImportVanillaTextureViewModel ImportVanillaTextureViewModel { get; set; }
        public VanillaFileViewModel VanillaFileViewModel { get; set; }

        readonly ItemListViewModel _itemListViewModel;
        readonly IWindowService _windowService;

        string? _selectedItemName;
        public string? SelectedItemName
        {
            get { return _selectedItemName; }
            set { _selectedItemName = value; OnPropertyChanged(); }
        }

        public ImportVanillaViewModel(IModsListViewModel modPack, ItemListViewModel itemListViewModel, VanillaFileService vanillaFileService, IWindowService windowService, ILogService logService)
            : base(logService)
        {
            ImportVanillaModelViewModel = new(modPack, vanillaFileService.ModelFileService, logService);
            ImportVanillaMaterialViewModel = new(modPack, vanillaFileService.MaterialFileService, logService);
            ImportVanillaMetadataViewModel = new(modPack, vanillaFileService.MetadataFileService, logService);
            ImportVanillaTextureViewModel = new(modPack, vanillaFileService.TextureFileService, logService);

            _windowService = windowService;

            VanillaFileViewModel = new(logService);

            _itemListViewModel = itemListViewModel;
            _itemListViewModel.PropertyChanged += new(OnItemListPropertyChanged);

            ImportVanillaMaterialViewModel.PropertyChanged += new(OnMaterialChanged);

            ImportVanillaModelViewModel.PropertyChanged += new(OnGameFileChanged);
            ImportVanillaMaterialViewModel.PropertyChanged += new(OnGameFileChanged);
        }

        private void OnGameFileChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ImportVanillaModelViewModel.SelectedModelFile))
            {
                VanillaFileViewModel.ModelFiles = new List<IModelGameFile>() { ImportVanillaModelViewModel.SelectedModelFile };
            }
            else if (e.PropertyName == nameof(ImportVanillaMaterialViewModel.MaterialFiles))
            {
                VanillaFileViewModel.MaterialFiles = ImportVanillaMaterialViewModel.MaterialFiles;
            }
            else if (e.PropertyName == nameof(ImportVanillaMaterialViewModel.MaterialsDict))
            {
                VanillaFileViewModel.MaterialsDict = ImportVanillaMaterialViewModel.MaterialsDict;
            }
        }

        private async void OnItemListPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ItemListViewModel.SelectedItem))
            {
                var item = _itemListViewModel.SelectedItem;
                SelectedItemName = item?.Name;

                var modelTask = ImportVanillaModelViewModel.SetItem(item);
                var materialTask = ImportVanillaMaterialViewModel.SetItem(item);
                var metadataTask = ImportVanillaMetadataViewModel.SetItem(item);

                var tasks = new Task[] { modelTask, materialTask, metadataTask };
                await Task.WhenAll(tasks);
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

        DelegateCommand _openInformationCommand;
        public DelegateCommand OpenInformationCommand
        {
            get { return _openInformationCommand ??= new DelegateCommand(_ => OpenInformation()); }
        }

        protected void OpenInformation()
        {
            if (!_windowService.IsWindowOpen<VanillaFileWindow>())
            {
                _windowService.Show<VanillaFileWindow>(VanillaFileViewModel);
            }
        }
    }
}
