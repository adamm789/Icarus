using Icarus.ViewModels.Export;
using Icarus.ViewModels.Mods.DataContainers;
using Icarus.ViewModels.Mods.DataContainers.Interfaces;
using Icarus.ViewModels.Util;
using Icarus.Views.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.ViewModels.Editor
{
    public class AdvancedEditorViewModel : NotifyPropertyChanged
    {
        public FilteredModsListViewModel FilteredModsListViewModel { get; }
        public IModPackViewModel ModPackViewModel { get; }
        public ExportViewModel ExportViewModel { get; }
        public ModPackListViewModel ModPackListViewModel { get; }

        public AdvancedEditorViewModel(IModPackViewModel modPackViewModel, ExportViewModel exportViewModel, ModPackListViewModel modPackListViewModel)
        {
            FilteredModsListViewModel = modPackViewModel.ModsListViewModel.FilteredModsList;
            ModPackViewModel = modPackViewModel;
            ExportViewModel = exportViewModel;
            ModPackListViewModel = modPackListViewModel;
        }
    }
}
