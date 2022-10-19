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
using xivModdingFramework.General.Enums;

namespace Icarus.ViewModels.Mods
{

    public class MetadataModViewModel : ModViewModel
    {
        IWindowService _windowService;
        MetadataMod _metadataMod;

        // TODO: Est
        public MetadataModViewModel(MetadataMod mod, IGameFileService gameFileDataService, IWindowService windowService, ILogService logService)
            : base(mod, gameFileDataService, logService)
        {
            _metadataMod = mod;
            _windowService = windowService;

            EqdpViewModel = new(_metadataMod.EqdpEntries);

            EstViewModel = new(_metadataMod.EstEntries);

            if (_metadataMod.EqpEntry != null)
            {
                EqpViewModel = new(_metadataMod.EqpEntry);
            }

            if (_metadataMod.GmpEntry != null)
            {
                GmpViewModel = new(_metadataMod.GmpEntry);
            }
        }
        public EqdpViewModel EqdpViewModel { get; }
        public EstViewModel EstViewModel { get; }
        public EqpViewModel EqpViewModel { get; }
        public GmpViewModel GmpViewModel { get; }


        DelegateCommand _openMetadataEditorCommand;
        public DelegateCommand OpenMetadataEditorCommand
        {
            get { return _openMetadataEditorCommand ??= new DelegateCommand(o => OpenMetadataEditor()); }
        }

        public void OpenMetadataEditor()
        {
            _windowService.ShowWindow<MetadataWindow>(this);
        }
    }
}
