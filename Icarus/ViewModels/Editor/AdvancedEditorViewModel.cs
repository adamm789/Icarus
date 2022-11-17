using Icarus.ViewModels.Export;
using Icarus.ViewModels.Mods.DataContainers;
using Icarus.ViewModels.Mods.DataContainers.Interfaces;
using Icarus.ViewModels.Mods.DataContainers.ModPackList;
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

        private void OnCopy()
        {
            if (ModPackListViewModel.ModPackPage is ModPackPageViewModel page)
            {
                // TODO: When on meta, copy page... copies everything?
                ModPackViewModel.CopyPage(page);
            }
            if (ModPackListViewModel.ModPackPage is ModPackMetaViewModel meta)
            {
                ModPackViewModel.SetMetadata(meta);
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
