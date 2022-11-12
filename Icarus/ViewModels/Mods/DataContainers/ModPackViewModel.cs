using GongSolutions.Wpf.DragDrop;
using Icarus.Mods.DataContainers;
using Icarus.Services;
using Icarus.Services.Interfaces;
using Icarus.ViewModels.Mods.DataContainers.Interfaces;
using Icarus.ViewModels.Util;
using Serilog;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Icarus.ViewModels.Mods.DataContainers
{
    public class ModPackViewModel : ViewModelBase, IModPackViewModel, IDropTarget
    {
        public ModPack ModPack { get; }
        public IModsListViewModel ModsListViewModel { get; }
        public IModPackMetaViewModel ModPackMetaViewModel { get; }
        // TODO: Ability to see all imported advanced modpacks and their options
        // TODO: Allow user to drag and drop groups, options, and pages to the current pack
        public ObservableCollection<ModPackPageViewModel> ModPackPages { get; } = new();

        // public ObservableCollection<INotifyPropertyChanged> ModPackPages { get; } = new();

        public bool IsReadOnly { get; }
        ViewModelService _viewModelService;

        //IModPackMetaViewModel _modPackMetaViewModel;

        public ModPackViewModel(ModPack modPack, ViewModelService viewModelService, ILogService logService,
            bool isReadOnly = false, IModsListViewModel? modsListViewModel = null) : base(logService)
        {
            ModPack = modPack;
            _viewModelService = viewModelService;
            IsReadOnly = isReadOnly;

            ModPackMetaViewModel = _viewModelService.GetModPackMetaViewModel(modPack, isReadOnly);
            ModsListViewModel = modsListViewModel ?? new ModsListViewModel(modPack, _viewModelService, logService);

            DisplayedViewModel = ModPackMetaViewModel;

            ModsListViewModel.SimpleModsList.CollectionChanged += new NotifyCollectionChangedEventHandler(OnCollectionChanged);
            foreach (var page in ModPack.ModPackPages)
            {
                var packPage = new ModPackPageViewModel(page, this, _viewModelService, IsReadOnly);
                packPage.PropertyChanged += new(OnOptionSelected);
                ModPackPages.Add(packPage);
            }
            IsReadOnly = isReadOnly;
        }

        public void SetMetadata(ModPackMetaViewModel meta)
        {
            ModPackMetaViewModel.CopyFrom(meta.ModPack);
        }

        private void OnOptionSelected(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ModPackPageViewModel.DisplayedOption) && sender is ModPackPageViewModel modPackPage)
            {
                SelectedOption = modPackPage.DisplayedOption;
            }
        }

        public List<int> GetAvailablePageIndices()
        {
            var ret = new List<int>();
            for (var i = 0; i < ModPackPages.Count + 1; i++)
            {
                ret.Add(i);
            }

            return ret;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems.Cast<ModViewModel>())
                {
                    RemoveMod(item);
                }
            }
        }

        public void Add(ModPack modPack)
        {
            ModsListViewModel.AddRange(modPack.SimpleModsList);
        }

        string _newOrNext = "New Page";
        public string NewOrNext
        {
            get { return _newOrNext; }
            set { _newOrNext = value; OnPropertyChanged(); }
        }

        // TODO: Debug page index
        int _pageIndex = 0;
        public int PageIndex
        {
            get { return _pageIndex; }
            set
            {
                _pageIndex = value;
                OnPropertyChanged();
                UpdatePagesView();
            }
        }

        // Used to disable "Previous Page" on the mod meta page
        bool _isNotFirstPage = false;
        public bool IsNotFirstPage
        {
            get { return _isNotFirstPage; }
            set { _isNotFirstPage = value; OnPropertyChanged(); }
        }

        // ModPackPageViewModel or ModPackMetaViewModel
        INotifyPropertyChanged _displayedViewModel;
        public INotifyPropertyChanged DisplayedViewModel
        {
            get { return _displayedViewModel; }
            set { _displayedViewModel = value; OnPropertyChanged(); }
        }

        ModOptionViewModel? _selectedOption;
        public ModOptionViewModel? SelectedOption
        {
            get { return _selectedOption;}
            set { _selectedOption = value; OnPropertyChanged(); }
        }

        DelegateCommand _decreaseCommand;
        public DelegateCommand DecreaseCommand
        {
            get { return _decreaseCommand ??= new DelegateCommand(o => DecreasePageIndex()); }
        }

        DelegateCommand _increaseCommand;
        public DelegateCommand IncreaseCommand
        {
            get { return _increaseCommand ??= new DelegateCommand(o => IncreasePageIndex()); }
        }

        public ModPackPageViewModel AddPage()
        {
            var page = new ModPackPageViewModel(ModPackPages.Count, this, _viewModelService);

            AddPage(page);
            return page;
        }

        public void AddPage(ModPackPageViewModel packPage)
        {
            ModPack.ModPackPages.Add(packPage.GetModPackPage());
            ModPackPages.Add(packPage);
            packPage.PropertyChanged += new PropertyChangedEventHandler(OnOptionSelected);
        }

        public void InsertPage(ModPackPageViewModel packPage, int index)
        {
            ModPack.ModPackPages.Insert(index, packPage.GetModPackPage());
            ModPackPages.Insert(index, packPage);
        }

        public void CopyPage(ModPackPageViewModel page)
        {
            /*
            var index = PageIndex;
            if (PageIndex < ModPackPages.Count && ModPackPages[PageIndex].IsEmpty())
            {
                var p = new ModPackPageViewModel(page, PageIndex, this);
                InsertPage(p, PageIndex);
            }
            else {
            */
            // TODO: If last page is empty, set that page to this one
                var p = new ModPackPageViewModel(page, ModPackPages.Count, this);
                AddPage(p);
            //}
        }

        public void RemovePage(ModPackPageViewModel packPage)
        {
            ModPack.ModPackPages.Remove(packPage.GetModPackPage());
            ModPackPages.Remove(packPage);

            UpdatePageIndices();
            PageIndex = packPage.PageIndex - 1;
        }

        public bool ArePagesEmpty()
        {
            foreach (var page in ModPackPages)
            {
                if (page.IsEmpty())
                {
                    continue;
                }
                return false;
            }
            return true;
        }

        public void Move(ModPackPageViewModel source, ModPackPageViewModel target)
        {
            // TODO: More error checks?
            var sourceIndex = ModPackPages.IndexOf(source);
            var targetIndex = ModPackPages.IndexOf(target);

            ModPackPages.Move(sourceIndex, targetIndex);
            ModPack.MovePage(sourceIndex, targetIndex);

            // Change to new page index if it has moved
            if (PageIndex == sourceIndex + 1)
            {
                PageIndex = targetIndex + 1;
            }
        }

        private void DecreasePageIndex()
        {
            PageIndex--;
        }

        // TODO: Inputting page (index + 1) to add a new page
        public void IncreasePageIndex()
        {
            if (PageIndex == ModPackPages.Count && ShouldAddPage())
            {
                AddPage();
            }
            if (PageIndex < ModPackPages.Count)
            {
                PageIndex++;
            }
        }

        private void UpdatePagesView()
        {
            if (PageIndex > 0)
            {
                if (PageIndex >= ModPackPages.Count)
                {
                    _pageIndex = ModPackPages.Count;
                    NewOrNext = "New Page";
                }
                else
                {
                    NewOrNext = "Next Page";
                }
                DisplayedViewModel = ModPackPages[PageIndex - 1];
                IsNotFirstPage = true;
            }
            else
            {
                if (PageIndex < 0)
                {
                    _pageIndex = 0;
                }
                NewOrNext = "Next Page";

                IsNotFirstPage = false;
                DisplayedViewModel = ModPackMetaViewModel;
            }
        }

        private bool ShouldAddPage()
        {
            if (PageIndex == 0)
            {
                return true;
            }
            else
            {
                var lastIndex = ModPackPages.Count - 1;
                return ModPackPages[lastIndex].ModGroups.Count > 0;
            }
        }

        private void UpdatePageIndices()
        {
            var index = 1;
            foreach (var page in ModPackPages)
            {
                page.PageIndex = index;
                index++;
            }
        }

        private void RemoveModViewModel(ModViewModel mod)
        {
            RemoveMod(mod);
        }

        /// <summary>
        /// Removes the <paramref name="mod"/> from the ModPackPages
        /// </summary>
        /// <param name="mod"/></param>
        /// <returns></returns>
        private int RemoveMod(ModViewModel mod)
        {
            var modsList = new List<ModViewModel>() { mod };
            var retVal = RemoveMods(modsList);
            return retVal;
        }

        private int RemoveMods(List<ModViewModel> mods)
        {
            int numRemoved = 0;

            foreach (var page in ModPackPages)
            {
                numRemoved += page.RemoveMods(mods);
            }
            Log.Verbose($"Removed {numRemoved} mods from pages.");
            return numRemoved;
        }

        // TODO: DragDrop for ModPack
        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            var source = dropInfo.Data;
            var target = dropInfo.TargetItem;
            if (source is ModPackPageViewModel && target is ModPackPageViewModel)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Copy;
            }
            else if (source is ModGroupViewModel)
            {

            }
            else
            {
                dropInfo.NotHandled = true;
            }
        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            var source = dropInfo.Data;
            var target = dropInfo.TargetItem;

            _logService.Debug($"Drop onto {target} from {GetType()}");
            if (source is ModPackPageViewModel sourcePage && target is ModPackPageViewModel targetPage)
            {
                Move(sourcePage, targetPage);
            }
            else
            {
                dropInfo.NotHandled = true;
            }
        }
    }
}
