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
using ItemDatabase.Paths;
using System.Text.RegularExpressions;

namespace Icarus.ViewModels.Import
{
    public class ImportVanillaMetadataViewModel : ImportVanillaFileViewModel
    {
        readonly IMetadataFileService _metadataFileService;
        private IMetadataFile? _metadataFile;
        public ImportVanillaMetadataViewModel(IModsListViewModel modPack, IMetadataFileService metadataFileService, ILogService logService) 
            : base(modPack, logService)
        {
            _metadataFileService = metadataFileService;
        }

        bool _hasMetadata = false;
        public bool HasMetadata
        {
            get { return _hasMetadata; }
            set { _hasMetadata = value; OnPropertyChanged(); }
        }

        public async override Task<IMetadataFile?> SetItem(IItem? item)
        {
            _metadataFile = await _metadataFileService.GetMetadata(item);
            HasMetadata = _metadataFile?.ItemMetadata != null;
            CanImport = HasMetadata;
            return _metadataFile;
        }

        public async override Task SetCompletePath(string? path)
        {
            await base.SetCompletePath(path);
            HasMetadata = false;

            if (String.IsNullOrWhiteSpace(path) || !Regex.IsMatch(path, @".meta$"))
            {
                CanImport = false;
            }
            else
            {
                _metadataFile = await _metadataFileService.TryGetMetadata(path);
                CanImport = _metadataFile != null;
            }
        }

        
        private MetadataMod? GetVanillaMeta()
        {
            if (_metadataFile != null)
            {
                var mod = new MetadataMod(_metadataFile, ImportSource.Vanilla);
                var modViewModel = _modPackViewModel.Add(mod);
                return mod;
            }
            return null;
        }

        protected override void DoImport()
        {
            GetVanillaMeta();
        }
    }
}
