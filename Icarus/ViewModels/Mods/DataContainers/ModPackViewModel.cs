using GongSolutions.Wpf.DragDrop;
using Icarus.Mods.DataContainers;
using Icarus.Services;
using Icarus.Services.Interfaces;
using Icarus.ViewModels.Mods.DataContainers.Interfaces;
using Icarus.ViewModels.Util;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO.Packaging;
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

        public bool IsReadOnly { get; }
        ViewModelService _viewModelService;

        public string Name => ModPackMetaViewModel.Name;

        public ModPackViewModel(ModPack modPack, ViewModelService viewModelService, ILogService logService,
            bool isReadOnly = false, IModsListViewModel? modsListViewModel = null) : base(logService)
        {
            ModPack = modPack;
            _viewModelService = viewModelService;
            IsReadOnly = isReadOnly;

            ModPackMetaViewModel = _viewModelService.GetModPackMetaViewModel(modPack, isReadOnly);
            ModsListViewModel = modsListViewModel ?? _viewModelService.GetModsListViewModel(modPack);

            DisplayedViewModel = ModPackMetaViewModel;

            ModsListViewModel.SimpleModsList.CollectionChanged += new NotifyCollectionChangedEventHandler(OnCollectionChanged);
            foreach (var page in ModPack.ModPackPages)
            {
                var packPage = new ModPackPageViewModel(page, this, _viewModelService, _logService, IsReadOnly);
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

        public List<string> GetAvailablePageIndices()
        {
            var ret = new List<string>();
            ret.Add("Metadata");
            for (var i = 0; i < ModPackPages.Count; i++)
            {
                ret.Add((i + 1).ToString());
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
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                Reset();
            }
        }

        public void Add(ModPack modPack)
        {
            ModsListViewModel.AddRange(modPack.SimpleModsList);
        }

        public void AddRange(IEnumerable<ModViewModel> mods)
        {
            ModsListViewModel.AddRange(mods);
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
            set
            {
                _displayedViewModel = value;
                OnPropertyChanged();
            }
        }

        ModOptionViewModel? _selectedOption;
        public ModOptionViewModel? SelectedOption
        {
            get { return _selectedOption; }
            set { _selectedOption = value; OnPropertyChanged(); }
        }

        DelegateCommand _showMetadataCommand;
        public DelegateCommand ShowMetadataCommand
        {
            get { return _showMetadataCommand ??= new DelegateCommand(_ => DisplayedViewModel = ModPackMetaViewModel); }
        }

        DelegateCommand _addPageCommand;
        public DelegateCommand AddPageCommand
        {
            get { return _addPageCommand ??= new DelegateCommand(_ => AddPage(), _ => CanAddPage()); }
        }

        ModPackPageViewModel? _selectedModPackPage;
        public ModPackPageViewModel? SelectedModPackPage
        {
            get { return _selectedModPackPage; }
            set
            {
                _selectedModPackPage = value;
                OnPropertyChanged();
                if (value != null)
                {
                    DisplayedViewModel = value;
                }
                else
                {
                    DisplayedViewModel = ModPackMetaViewModel;
                }
            }
        }

        protected bool CanAddPage()
        {
            var page = ModPackPages.LastOrDefault();
            if (page == null)
            {
                return true;
            }
            else
            {
                return !page.IsEmpty();
            }
        }

        public ModPackPageViewModel AddPage()
        {
            var page = new ModPackPageViewModel(ModPackPages.Count, this, _viewModelService, _logService);

            AddPage(page);
            SelectedModPackPage = page;
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

            UpdatePageIndices();
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

            // TODO: When copying pages, if there are no pack pages, the index starts at 0
            var pageIndex = ModPackPages.Count;

            var p = new ModPackPageViewModel(page, this, _logService);
            p.PropertyChanged += new PropertyChangedEventHandler(OnOptionSelected);
            if (ModPackPages.Count > 0 && ModPackPages[ModPackPages.Count - 1].HasZeroOptions)
            {
                ModPackPages[ModPackPages.Count - 1] = p;
                p.PageIndex = pageIndex;
                UpdatePageIndices();
            }
            else
            {
                InsertPage(p, pageIndex);
            }
            //}
        }

        public void RemovePage(ModPackPageViewModel packPage)
        {
            var index = packPage.PageIndex - 1;
            ModPack.ModPackPages.Remove(packPage.GetModPackPage());
            ModPackPages.Remove(packPage);

            UpdatePageIndices();
            if (index <= 0 )
            {
                SelectedModPackPage = null;
                DisplayedViewModel = ModPackMetaViewModel;
            }
            else
            {
                SelectedModPackPage = ModPackPages[index - 1];
            }
        }

        protected void UpdatePageIndices()
        {
            for (var i = 0; i < ModPackPages.Count; i++)
            {
                var page = ModPackPages[i];
                page.PageIndex = i + 1;
            }
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
            if (source.HasZeroOptions || target.HasZeroOptions)
            {
                _logService?.Debug($"Trying to move an empty pack page.");
                return;
            }
            // TODO: More error checks?
            var sourceIndex = ModPackPages.IndexOf(source);
            var targetIndex = ModPackPages.IndexOf(target);

            _logService?.Debug($"Moving page index {sourceIndex} to page index {targetIndex}.");

            ModPackPages.Move(sourceIndex, targetIndex);
            ModPack.MovePage(sourceIndex, targetIndex);

            source.PageIndex = targetIndex + 1;
            target.PageIndex = sourceIndex + 1;

            // Change to new page index if it has moved
            /**
            if (PageIndex == sourceIndex + 1)
            {
                PageIndex = targetIndex + 1;
            }
            */
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
            _logService?.Verbose($"Removed {numRemoved} mods from pages.");
            return numRemoved;
        }

        public void Reset()
        {
            _logService?.Information($"Clearing mod pack. Removing all pages.");
            SelectedModPackPage = null;
            DisplayedViewModel = ModPackMetaViewModel;
            ModPackPages.Clear();
        }

        // TODO: DragDrop for ModPack
        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            var source = dropInfo.Data;
            var target = dropInfo.TargetItem;
            if (source is ModPackPageViewModel sourcePage && target is ModPackPageViewModel && !sourcePage.HasZeroOptions)
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

            _logService?.Debug($"Drop onto {target} from {GetType()}");
            if (source is ModPackPageViewModel sourcePage && target is ModPackPageViewModel targetPage)
            {
                Move(sourcePage, targetPage);
            }
            else
            {
                dropInfo.NotHandled = true;
            }
        }
        // TODO: Allow reordering pages
    }
}
