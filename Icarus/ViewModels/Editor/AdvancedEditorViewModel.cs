using Icarus.ViewModels.Export;
using Icarus.ViewModels.ModPackList;
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
        // TODO: Double click on mod to bring to simple editor?
        // TODO: CanExport with an incomplete mod in simple editor but not used in any mod pack page
        // TODO: Way to edit (or at least view) mods from advanced editor screen

        // TODO: How to transfer mod groups to new page?

        // TODO: Edit option name from within and add a "tab/"label" thing to re-order

        // TODO: "Copy Page" should change to "Copy Metadata" on first (metadata) page

        // TODO: Allow advanced modpacks to be create when all of the mods in the pack pages are complete
        // Currently, it requires ALL mods to complete, not just ones that are contained in the advanced pack
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

            ModPackViewModel.PropertyChanged += new(OnSelectedOptionChange);
        }

        private void OnDisplayedViewModelChange(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ModPackPageViewModel.DisplayedOption) && sender is ModPackPageViewModel modPackPage)
            {
                DisplayedOption = modPackPage.DisplayedOption;
                SelectedTabIndex = 0;
            }
        }

        private void OnSelectedOptionChange(object sender, PropertyChangedEventArgs e){
            if (e.PropertyName == nameof(ModPackViewModel.SelectedOption) && sender is ModPackViewModel modPack)
            {
                DisplayedOption = modPack.SelectedOption;
                SelectedTabIndex = 0;
            }
        }


        ModOptionViewModel? _displayedOption;
        public ModOptionViewModel? DisplayedOption
        {
            get { return _displayedOption; }
            set { _displayedOption = value; OnPropertyChanged(); }
        }



        int _selectedTabIndex = 0;
        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set { _selectedTabIndex = value; OnPropertyChanged(); }
        }
    }
}
