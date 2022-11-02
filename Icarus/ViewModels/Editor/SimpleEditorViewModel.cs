using Icarus.ViewModels.Export;
using Icarus.ViewModels.Import;
using Icarus.ViewModels.Items;
using Icarus.ViewModels.Mods.DataContainers;
using Icarus.ViewModels.Mods.DataContainers.Interfaces;
using Icarus.ViewModels.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.ViewModels.Editor
{
    public class SimpleEditorViewModel : NotifyPropertyChanged
    {
        public ImportViewModel ImportViewModel { get; }
        public IModPackMetaViewModel ModPackMetaViewModel { get; }
        public IModsListViewModel ModsListViewModel { get; }
        public ExportViewModel ExportViewModel { get; }
        public ItemListViewModel ItemListViewModel { get; }
        public ImportVanillaViewModel ImportVanillaViewModel { get; }

        public SimpleEditorViewModel(IModPackViewModel modPack, ItemListViewModel itemList, ImportVanillaViewModel importVanilla, ImportViewModel import, ExportViewModel export)
        {
            ModPackMetaViewModel = modPack.ModPackMetaViewModel;
            ModsListViewModel = modPack.ModsListViewModel;

            ItemListViewModel = itemList;
            ImportViewModel = import;
            ImportVanillaViewModel = importVanilla;
            ExportViewModel = export;
        }
    }
}
