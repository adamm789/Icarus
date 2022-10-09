﻿using GongSolutions.Wpf.DragDrop;
using Icarus.Mods.DataContainers;
using Icarus.Services;
using Icarus.ViewModels.Mods.DataContainers.Interfaces;
using Icarus.ViewModels.Util;
using Serilog;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;

namespace Icarus.ViewModels.Mods.DataContainers
{
    public class ModPackViewModel : NotifyPropertyChanged, IModPackViewModel, IDropTarget
    {
        public ModPack ModPack { get; }
        public IModsListViewModel ModsListViewModel { get; }
        ViewModelService _viewModelService;

        IModPackMetaViewModel _modPackMetaViewModel;
        public IModPackMetaViewModel ModPackMetaViewModel
        {
            get { return _modPackMetaViewModel; }
            set { _modPackMetaViewModel = value; OnPropertyChanged(); }
        }

        public ModPackViewModel(ModPack modPack, ViewModelService viewModelService)
        {
            ModPack = modPack;
            _viewModelService = viewModelService;

            ModPackMetaViewModel = _viewModelService.GetModPackMetaViewModel(modPack);
            ModsListViewModel = new ModsListViewModel(modPack, _viewModelService);

            DisplayedViewModel = ModPackMetaViewModel;
            ModsListViewModel.SimpleModsList.CollectionChanged += new NotifyCollectionChangedEventHandler(OnCollectionChanged);
        }


        public ModPackViewModel(ModPack modPack, IModPackMetaViewModel modPackMeta, IModsListViewModel modsList, ViewModelService viewModelService)
        {
            ModsListViewModel = modsList;
            _viewModelService = viewModelService;
            ModPack = modPack;
            ModPackMetaViewModel = modPackMeta;

            DisplayedViewModel = ModPackMetaViewModel;
            ModsListViewModel.SimpleModsList.CollectionChanged += new NotifyCollectionChangedEventHandler(OnCollectionChanged);
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    var mod = item as ModViewModel;
                    RemoveMod(mod);
                }
            }
        }

        public void SetModPack(ModPack modPack)
        {
            SetModPack(modPack, GetDefaultFlags());
        }

        // TODO: pages -> overwrite, insert at index, and append to end
        public void SetModPack(ModPack modPack, ModPackViewModelImportFlags flags)
        {
            ModsListViewModel.AddRange(modPack.SimpleModsList);
            if (flags.HasFlag(ModPackViewModelImportFlags.OverwriteData))
            {
                ModPack.CopyFrom(modPack);
            }
            if (flags.HasFlag(ModPackViewModelImportFlags.OverwritePages))
            {
                ModPack.ModPackPages.Clear();
                ModPackPages.Clear();
            }
            if (flags.HasFlag(ModPackViewModelImportFlags.AppendPagesToEnd))
            {
                foreach (var page in modPack.ModPackPages)
                {
                    var pageViewModel = new ModPackPageViewModel(page, this, _viewModelService);
                    AddPage(pageViewModel);
                }
                UpdatePagesView();
            }
            // TODO: Append to start?

            // TODO: How do I want to handle pre-existing mods?
            // Probably not delete them. So just append them to the SimpleModsList
            // The pages could be "insert at index"
        }

        public static ModPackViewModelImportFlags GetDefaultFlags()
        {
            return ModPackViewModelImportFlags.OverwriteData |
                ModPackViewModelImportFlags.OverwritePages | ModPackViewModelImportFlags.AppendPagesToEnd;
        }
        // TODO: Ability to see all imported advanced modpacks and their options
        // TODO: Allow user to drag and drop groups, options, and pages to the current pack
        ObservableCollection<ModPackPageViewModel> _modPackPages = new();
        public ObservableCollection<ModPackPageViewModel> ModPackPages
        {
            get { return _modPackPages; }
            set { _modPackPages = value; OnPropertyChanged(); }
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

        INotifyPropertyChanged _displayedViewModel;
        public INotifyPropertyChanged DisplayedViewModel
        {
            get { return _displayedViewModel; }
            set { _displayedViewModel = value; OnPropertyChanged(); }
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
        }

        private void DecreasePageIndex()
        {
            PageIndex--;
        }

        // TODO: Inputting page index +1 to add a new page
        public void IncreasePageIndex()
        {
            if (PageIndex == ModPackPages.Count && ShouldAddPage())
            {
                AddPage();
            }
            PageIndex++;
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
            if (DisplayedViewModel is ModPackPageViewModel modPackPage)
            {
                if (modPackPage.ModGroups.Count > 0)
                {
                    return true;
                }
            }
            if (DisplayedViewModel is ModPackMetaViewModel)
            {
                return true;
            }
            return false;
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
        /// Removes a mod from the ModPackPages
        /// </summary>
        /// <param name="mod"></param>
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
            Log.Information($"Removed {numRemoved} mods from pages.");
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

        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            var source = dropInfo.Data;
            var target = dropInfo.TargetItem;

            if (source is ModPackPageViewModel sourcePage && target is ModPackPageViewModel targetPage)
            {
                Move(sourcePage, targetPage);
            }
        }
    }
}
