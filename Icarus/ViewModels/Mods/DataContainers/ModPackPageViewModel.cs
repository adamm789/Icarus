using GongSolutions.Wpf.DragDrop;
using Icarus.Mods.DataContainers;
using Icarus.Services;
using Icarus.Util.Extensions;
using Icarus.ViewModels.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace Icarus.ViewModels.Mods.DataContainers
{
    public class ModPackPageViewModel : NotifyPropertyChanged, IDropTarget
    {
        protected ModPackPage _modPackPage;
        readonly ViewModelService _viewModelService;
        public ObservableCollection<ModGroupViewModel> ModGroups { get; } = new();
        public DelegateCommand? RemoveCommand { get; set; }
        public bool IsReadOnly = false;

        public ModPackPageViewModel(ModPackPageViewModel other, int index, ModPackViewModel parent)
        {
            _modPackPage = new(other._modPackPage);
            _viewModelService = other._viewModelService;
            RemoveCommand = new(o => parent.RemovePage(this));
            foreach (var group in other.ModGroups)
            {
                var newGroup = new ModGroupViewModel(group, this);
                AddGroup(newGroup);
            }
        }

        public ModPackPageViewModel(int index, ModPackViewModel parent, ViewModelService viewModelService)
        {
            _modPackPage = new(index + 1);
            _viewModelService = viewModelService;
            RemoveCommand = new(o => parent.RemovePage(this));
        }

        public ModPackPageViewModel(ModPackPage page, ModPackViewModel parent, ViewModelService viewModelService, bool isReadOnly = false)
        {
            _modPackPage = new(page);
            _viewModelService = viewModelService;
            IsReadOnly = isReadOnly;
            foreach (var group in page.ModGroups)
            {
                //var groupViewModel = new ModGroupViewModel(group, gameFileDataService, windowService);
                var groupViewModel = new ModGroupViewModel(group, this, _viewModelService, IsReadOnly);
                AddGroup(groupViewModel);
            }
            RemoveCommand = new(o => parent.RemovePage(this));
            IsReadOnly = isReadOnly;
        }

        private void OnSelectedOption(object sender, PropertyChangedEventArgs e)
        {
            if (sender is ModGroupViewModel vm)
            {
                DisplayedOption = vm.SelectedOption;
            }
        }

        public int PageIndex
        {
            get { return _modPackPage.PageIndex; }
            set { _modPackPage.PageIndex = value; OnPropertyChanged(); }
        }

        ModOptionViewModel _displayedOption;
        public ModOptionViewModel DisplayedOption
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

        public void AddGroup(ModGroupViewModel group)
        {
            ModGroups.Add(group);
            _modPackPage.AddGroup(group.GetGroup());
            group.RemoveCommand = new(o => RemoveGroup(group));
            group.PropertyChanged += new PropertyChangedEventHandler(OnSelectedOption);
        }

        public void RemoveGroup(ModGroupViewModel group)
        {
            ModGroups.Remove(group);
            _modPackPage.ModGroups.Remove(group.GetGroup());
            group.OnRemove();
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

        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            var source = dropInfo.Data;
            var target = dropInfo.TargetItem;

            if (source is ModOptionViewModel sourceOption && target is ModOptionViewModel targetOption)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Copy;
            }
            else if (source is ModGroupViewModel && target is ModGroupViewModel)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Copy;
            }
            else if (source is ModGroupViewModel)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Copy;
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
                    targetGroup.AddOption(sourceOption);
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
