﻿using GongSolutions.Wpf.DragDrop;
using Icarus.Mods.DataContainers;
using Icarus.Services;
using Icarus.Util.Extensions;
using Icarus.ViewModels.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using Serilog;
using System.Windows.Data;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using System.Collections.Specialized;
using Icarus.Services.Interfaces;
using static Lumina.Data.Parsing.Uld.UldRoot;

namespace Icarus.ViewModels.Mods.DataContainers
{
    public class ModPackPageViewModel : ViewModelBase, IDropTarget
    {
        protected ModPackPage _modPackPage;
        readonly ViewModelService _viewModelService;
        public ObservableCollection<ModGroupViewModel> ModGroups { get; } = new();
        public DelegateCommand? RemoveCommand { get; set; }
        public bool IsReadOnly = false;

        public ModPackPageViewModel(ModPackPageViewModel other, ModPackViewModel parent, ILogService logService) : base(logService)
        {
            _modPackPage = new(other._modPackPage);
            _viewModelService = other._viewModelService;
            RemoveCommand = new(o => parent.RemovePage(this));
            var hasZeroOptions = true;

            foreach (var group in other.ModGroups)
            {
                var newGroup = new ModGroupViewModel(group, this);
                AddGroup(newGroup);
                if (!newGroup.HasZeroOptions)
                {
                    hasZeroOptions = false;
                }
            }

            HasZeroOptions = hasZeroOptions;
            ModGroups.CollectionChanged += new NotifyCollectionChangedEventHandler(OnModGroupCollectionChanged);
        }

        public ModPackPageViewModel(int index, ModPackViewModel parent, ViewModelService viewModelService, ILogService logService) : base(logService)
        {
            _modPackPage = new(index + 1);
            _viewModelService = viewModelService;
            RemoveCommand = new(o => parent.RemovePage(this));
            ModGroups.CollectionChanged += new NotifyCollectionChangedEventHandler(OnModGroupCollectionChanged);
        }

        public ModPackPageViewModel(ModPackPage page, ModPackViewModel parent, ViewModelService viewModelService, ILogService logService, bool isReadOnly = false) : base(logService)
        {
            _modPackPage = new(page);
            _viewModelService = viewModelService;
            IsReadOnly = isReadOnly;
            var hasZeroOptions = true;

            foreach (var group in page.ModGroups)
            {
                var groupViewModel = new ModGroupViewModel(group, this, _viewModelService, IsReadOnly);
                AddGroup(groupViewModel);
                if (!groupViewModel.HasZeroOptions)
                {
                    hasZeroOptions = false;
                }
            }
            HasZeroOptions = hasZeroOptions;

            ModGroups.CollectionChanged += new NotifyCollectionChangedEventHandler(OnModGroupCollectionChanged);
            RemoveCommand = new(o => parent.RemovePage(this));
        }

        ModGroupViewModel _previousGroup;
        private void OnModGroupPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is ModGroupViewModel vm && e.PropertyName == nameof(ModGroupViewModel.DisplayedOption) && vm.DisplayedOption != null)
            {
                if (_previousGroup != null && _previousGroup != vm) _previousGroup.DisplayedOption = null;
                DisplayedOption = vm.DisplayedOption;
                _previousGroup = vm;
            }
            if (e.PropertyName == nameof(ModGroupViewModel.HasZeroOptions))
            {
                foreach (var group in ModGroups)
                {
                    if (!group.HasZeroOptions)
                    {
                        HasZeroOptions = false;
                        return;
                    }
                }
                HasZeroOptions = true;
            }
        }

        /// <summary>
        /// The 1-based index of the page
        /// </summary>
        public int PageIndex
        {
            get { return _modPackPage.PageIndex; }
            set { _modPackPage.PageIndex = value; OnPropertyChanged(); }
        }

        ModOptionViewModel? _displayedOption;
        public ModOptionViewModel? DisplayedOption
        {
            get { return _displayedOption; }
            set { _displayedOption = value; OnPropertyChanged(); }
        }

        string _newGroupName;
        public string NewGroupName
        {
            get { return _newGroupName; }
            set { _newGroupName = value; OnPropertyChanged(); }
        }

        DelegateCommand _addGroupCommand;
        public DelegateCommand AddGroupCommand
        {
            get { return _addGroupCommand ??= new DelegateCommand(o => AddGroup()); }
        }

        public ModGroupViewModel AddGroup(string groupName)
        {
            var vm = new ModGroupViewModel(groupName, this, _viewModelService, IsReadOnly);
            ModGroups.Add(vm);
            return vm;
        }

        private Dictionary<ModGroupViewModel, PropertyChangedEventHandler> _selectedOptionsDict = new();

        public void AddGroup(ModGroupViewModel group)
        {
            if (_selectedOptionsDict.ContainsKey(group))
            {
                group = new ModGroupViewModel(group, this);
            }
            HasZeroOptions = HasZeroOptions && group.HasZeroOptions;
            ModGroups.Add(group);
            _modPackPage.AddGroup(group.GetGroup());
            group.RemoveCommand = new(o => RemoveGroup(group));

            var eh = new PropertyChangedEventHandler(OnModGroupPropertyChanged);
            group.PropertyChanged += eh;

            _selectedOptionsDict.Add(group, eh);
        }

        public void RemoveGroup(ModGroupViewModel group)
        {
            ModGroups.Remove(group);
            _modPackPage.ModGroups.Remove(group.GetGroup());
            group.OnRemove();

            var eh = _selectedOptionsDict[group];
            group.PropertyChanged -= eh;

            _selectedOptionsDict.Remove(group);
        }

        public int RemoveMods(List<ModViewModel> mods)
        {
            var numRemoved = 0;
            foreach (var group in ModGroups)
            {
                numRemoved += group.RemoveMods(mods);
            }
            return numRemoved;
        }

        private void AddGroup()
        {
            if (!string.IsNullOrWhiteSpace(NewGroupName))
            {
                NewGroupName = NewGroupName.Trim();
                var vm = new ModGroupViewModel(NewGroupName, this, _viewModelService, IsReadOnly);

                AddGroup(vm);
                NewGroupName = string.Empty;
            }
        }

        private void OnModGroupCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (ModGroups.Count == 0)
            {
                HasZeroOptions = true;
            }
        }

        public ModPackPage GetModPackPage()
        {
            return _modPackPage;
        }

        public void MoveTo(ModGroupViewModel oldGroup, ModGroupViewModel newGroup)
        {
            var oldIndex = ModGroups.IndexOf(oldGroup);
            var newIndex = ModGroups.IndexOf(newGroup);

            MoveTo(oldIndex, newIndex);
        }

        public void MoveTo(int oldIndex, int newIndex)
        {
            if (oldIndex < 0 || oldIndex > ModGroups.Count || newIndex < 0 || newIndex > ModGroups.Count)
            {
                _logService?.Debug($"Invalid oldIndex: {oldIndex} - newIndex: {newIndex}");
                return;
            }

            ModGroups.Move(oldIndex, newIndex);
            _modPackPage.ModGroups.Move(oldIndex, newIndex);
        }

        public bool IsEmpty()
        {
            foreach (var group in ModGroups)
            {
                if (group.IsEmpty())
                {
                    continue;
                }
                return false;
            }
            return true;
        }

        bool _hasZeroOptions = true;
        public bool HasZeroOptions
        {
            get { return _hasZeroOptions; }
            set { _hasZeroOptions = value; OnPropertyChanged(); }
        }


        public int GetNumMods()
        {
            var ret = 0;
            foreach (var group in ModGroups)
            {
                ret += group.OptionList.Count;
            }
            return ret;
        }

        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            var source = dropInfo.Data;
            var target = dropInfo.TargetItem;

            if (source is ModOptionViewModel && target is ModOptionViewModel)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Move;
            }
            else if (source is ModGroupViewModel && target is ModGroupViewModel)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Move;
            }
            else if (source is ModGroupViewModel)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Copy;
            }
            else
            {
                dropInfo.NotHandled = false;
            }
        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            var source = dropInfo.Data;
            var target = dropInfo.TargetItem;
            Log.Debug($"Drop {source} onto {target} in {GetType()}");
            if (target is ModGroupViewModel targetGroup)
            {
                // TODO: BEtter method for determining if MoveTo should be called
                // i.e. from the ModPackListViewModel, groups and options shouldn't be removed
                if (source is ModGroupViewModel sourceGroup)
                {
                    if (!sourceGroup.IsReadOnly)
                    {
                        MoveTo(sourceGroup, targetGroup);
                    }
                    else
                    {
                        var newGroup = new ModGroupViewModel(sourceGroup, this);
                        AddGroup(newGroup);
                    }
                }
                else if (source is ModOptionViewModel sourceOption)
                {
                    if (sourceOption.IsReadOnly)
                    {
                        targetGroup.CopyOption(sourceOption);
                    }
                    else
                    {
                        if (sourceOption.RemoveCommand != null)
                        {
                            sourceOption.RemoveCommand.Execute(sourceOption);
                            targetGroup.AddOption(sourceOption);
                        }
                        else
                        {
                            dropInfo.NotHandled = false;
                            _logService?.Error($"RemoveCommand for option was null.");
                        }
                    }
                }
                else
                {
                    dropInfo.NotHandled = false;
                }
            }
            else if (source is ModGroupViewModel sourceGroup && target == null)
            {
                if (sourceGroup.IsReadOnly)
                {
                    var newGroup = new ModGroupViewModel(sourceGroup, this);
                    AddGroup(newGroup);
                }
                else
                {
                    AddGroup(sourceGroup);
                }
            }
            else
            {
                dropInfo.NotHandled = false;
            }
        }
    }
}
