using Icarus.Mods.Interfaces;
using Icarus.Mods;
using Icarus.Services.GameFiles;
using Icarus.Services.GameFiles.Interfaces;
using Icarus.Services.Interfaces;
using Icarus.Util.Import;
using Icarus.ViewModels.Items;
using Icarus.ViewModels.Mods.DataContainers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ItemDatabase.Interfaces;

namespace Icarus.ViewModels.Import
{
    public class ImportVanillaMetadataViewModel : ImportVanillaFileViewModel
    {
        readonly IMetadataFileService _metadataFileService;
        private IMetadataFile? _metadataFile;
        public ImportVanillaMetadataViewModel(IModsListViewModel modPack, ItemListViewModel itemListService, IMetadataFileService metadataFileService, ILogService logService) 
            : base(modPack, itemListService, logService)
        {
            _metadataFileService = metadataFileService;
        }

        bool _hasMetadata = false;
        public bool HasMetadata
        {
            get { return _hasMetadata; }
            set { _hasMetadata = value; OnPropertyChanged(); }
        }

        public async Task<IMetadataFile?> SetItem(IItem item)
        {
            SelectedItem = item;
            _metadataFile = await _metadataFileService.GetMetadata(SelectedItem);
            HasMetadata = _metadataFile?.ItemMetadata != null;
            CanImport = HasMetadata;
            return _metadataFile;
        }

        /*
        public override async Task SelectedItemSetAsync()
        {
            await base.SelectedItemSetAsync();

            var metadataFile = await _metadataFileService.GetMetadata(SelectedItem);
            HasMetadata = metadataFile?.ItemMetadata != null;
        }
        */

        private async Task<MetadataMod?> GetVanillaMeta()
        {
            if (_metadataFile == null && _completePath != null)
            {
                _metadataFile = await _metadataFileService.TryGetMetadata(_completePath, SelectedItemName);
            }

            if (_metadataFile != null)
            {
                var mod = new MetadataMod(_metadataFile, ImportSource.Vanilla);
                var modViewModel = _modPackViewModel.Add(mod);
                return mod;
            }
            return null;

            /*
            IMetadataFile? metadataFile;
            if (SelectedItem == null) return null;
            if (_completePath != null)
            {
                metadataFile = await _metadataFileService.TryGetMetadata(_completePath, SelectedItemName);
            }
            else
            {
                metadataFile = await _metadataFileService.GetMetadata(SelectedItem);
            }
            if (metadataFile != null)
            {
                var mod = new MetadataMod(metadataFile, ImportSource.Vanilla);
                var modViewModel = _modPackViewModel.Add(mod);
                return mod;
            }
            return null;
            */
        }

        protected override async Task DoImport()
        {
            await GetVanillaMeta();
        }
    }
}
