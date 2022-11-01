using Icarus.Mods;
using Icarus.Mods.Interfaces;
using Icarus.Services;
using Icarus.Services.GameFiles;
using Icarus.Services.GameFiles.Interfaces;
using Icarus.Services.Interfaces;
using Icarus.ViewModels.Mods.Metadata;
using Icarus.ViewModels.Util;
using Icarus.Views.Mods.Metadata;
using ItemDatabase.Paths;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using xivModdingFramework.General.Enums;

namespace Icarus.ViewModels.Mods
{
    // TODO: Double check that there are actually changing the metadata mod
    public class MetadataModViewModel : ModViewModel
    {
        IWindowService _windowService;
        MetadataMod _metadataMod;

        public MetadataModViewModel(MetadataMod mod, IGameFileService gameFileDataService, IWindowService windowService, ILogService logService)
            : base(mod, gameFileDataService, logService)
        {
            _metadataMod = mod;
            _windowService = windowService;

            EqdpViewModel = new(_metadataMod.EqdpEntries);

            if (_metadataMod.EqpEntry != null)
            {
                EqpViewModel = new(_metadataMod.EqpEntry);
            }

            EstViewModel = new(_metadataMod, EqdpViewModel);

            if (_metadataMod.GmpEntry != null)
            {
                GmpViewModel = new(_metadataMod.GmpEntry);
            }

            // TOOD: ImcEntries
            ImcViewModel = new(_metadataMod.ImcEntries);
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
                var ret = base.SetModData(gameFile);

                // TODO: Figure out EstEntries...
                EstViewModel.SetAllSkelId(metaFile.ItemMetadata.EstEntries);
                return ret;
            }
            return false;
        }
    }
}
