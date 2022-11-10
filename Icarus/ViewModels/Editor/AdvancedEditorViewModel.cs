using Icarus.ViewModels.Export;
using Icarus.ViewModels.Mods.DataContainers;
using Icarus.ViewModels.Mods.DataContainers.Interfaces;
using Icarus.ViewModels.Util;
using Icarus.Views.Mods;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.ViewModels.Editor
{
    public class AdvancedEditorViewModel : NotifyPropertyChanged
    {
        // TODO: Clean up the mess in AdvancedEditorView.xaml
        public FilteredModsListViewModel FilteredModsListViewModel { get; }
        public IModPackViewModel ModPackViewModel { get; }
        public ExportViewModel ExportViewModel { get; }
        public ModPackListViewModel ModPackListViewModel { get; }

        // TODO: Change the Tab to "Option" when an Option is selected
        public AdvancedEditorViewModel(IModPackViewModel modPackViewModel, ExportViewModel exportViewModel, ModPackListViewModel modPackListViewModel)
        {
            FilteredModsListViewModel = modPackViewModel.ModsListViewModel.FilteredModsList;
            ModPackViewModel = modPackViewModel;
            ExportViewModel = exportViewModel;
            ModPackListViewModel = modPackListViewModel;

            ModPackListViewModel.CopyPageCommand = new DelegateCommand(o => OnCopy());
            ModPackViewModel.DisplayedViewModel.PropertyChanged += new(OnDisplayedViewModelChange);
        }

        private void OnDisplayedViewModelChange(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ModPackPageViewModel.DisplayedOption))
            {
                SelectedTabIndex = 0;
            }
        }

        private void OnCopy()
        {
            if (ModPackListViewModel.ModPackPage is ModPackPageViewModel page)
            {
                // TODO: When on meta, copy page... copies everything?
                ModPackViewModel.CopyPage(page);
            }
        }

        int _selectedTabIndex = 0;
        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set { _selectedTabIndex = value; OnPropertyChanged(); }
        }
    }
}
