using Icarus.Mods;
using Icarus.Mods.Interfaces;
using Icarus.Services;
using Icarus.Services.GameFiles;
using Icarus.Services.GameFiles.Interfaces;
using Icarus.Services.Interfaces;
using Icarus.ViewModels.Mods.Metadata;
using Icarus.ViewModels.Util;
using Icarus.Views.Mods.Metadata;
using ItemDatabase.Interfaces;
using ItemDatabase.Paths;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using xivModdingFramework.General.Enums;

namespace Icarus.ViewModels.Mods
{
    // imc?
    // TODO: Double check that there are actually changing the metadata mod
    public class MetadataModViewModel : ModViewModel
    {
        IWindowService _windowService;
        MetadataMod _metadataMod;
        IMetadataFileService _metadataFileService;

        public MetadataModViewModel(MetadataMod mod, IMetadataFileService metadataFileService, IWindowService windowService, ILogService logService)
            : base(mod, metadataFileService, logService)
        {
            _metadataMod = mod;
            _windowService = windowService;
            _metadataFileService = metadataFileService;

            if (_metadataMod.EqdpEntries.Count > 0)
            {
                EqdpViewModel = new(_metadataMod.EqdpEntries);
            }

            if (_metadataMod.EqpEntry != null)
            {
                EqpViewModel = new(_metadataMod.EqpEntry);
            }

            if (_metadataMod.EstEntries.Count > 0)
            {
                EstViewModel = new(_metadataMod, EqdpViewModel);
            }

            if (_metadataMod.GmpEntry != null)
            {
                GmpViewModel = new(_metadataMod.GmpEntry);
            }

            if (_metadataMod.ImcEntries.Count > 0) {
                ImcViewModel = new(_metadataMod.ImcEntries, _metadataMod.ItemMetadata.Root);
            }
        }
        public EqdpViewModel EqdpViewModel { get; }
        public EstViewModel EstViewModel { get; }
        public EqpViewModel EqpViewModel { get; }
        public GmpViewModel GmpViewModel { get; }
        public ImcViewModel ImcViewModel { get; }


        DelegateCommand _openMetadataEditorCommand;
        public DelegateCommand OpenMetadataEditorCommand
        {
            get { return _openMetadataEditorCommand ??= new DelegateCommand(o => OpenMetadataEditor()); }
        }

        public void OpenMetadataEditor()
        {
            _windowService.ShowWindow<MetadataWindow>(this);
        }

        public override bool SetModData(IGameFile gameFile)
        {
            if (gameFile is IMetadataFile metaFile)
            {
                if (_metadataMod.ItemMetadata.Root.Info.Slot != metaFile.Slot)
                {
                    _logService.Error($"Unable to assign metadata to a different slot.");
                    return false;
                }
                var ret = base.SetModData(gameFile);

                // TODO: Figure out EstEntries...
                EstViewModel.SetAllSkelId(metaFile.ItemMetadata.EstEntries);
                return ret;
            }
            return false;
        }

        public override async Task<IGameFile?> GetFileData(IItem? itemArg = null)
        {
            return await _metadataFileService.GetMetadata(itemArg);
        }

        public override async Task<IGameFile?> GetFileData(string path, string name ="")
        {
            return await _metadataFileService.TryGetMetadata(path, name);
        }

        protected override bool HasValidPathExtension(string path)
        {
            return Path.GetExtension(path) == ".meta";
        }
    }
}
