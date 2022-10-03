﻿using GongSolutions.Wpf.DragDrop;
using Icarus.Mods.DataContainers;
using Icarus.Mods.Interfaces;
using Icarus.Services;
using Icarus.Services.GameFiles;
using Icarus.Services.Interfaces;
using Icarus.Util.Extensions;
using Icarus.ViewModels.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace Icarus.ViewModels.Mods.DataContainers
{
    // TODO: Allow drag/drop to reorder mod groups
    public class ModGroupViewModel : NotifyPropertyChanged, IDropTarget
    {
        protected ModGroup _modGroup;
        ViewModelService _modFileService;

        public ModGroupViewModel(string name, ModPackPageViewModel parent, ViewModelService modFileService)
        {
            _modFileService = modFileService;
            _modGroup = new(name);
            RemoveCommand = new DelegateCommand(o => parent.RemoveGroup(this));
        }
        public ModGroupViewModel(ModGroup group, ModPackPageViewModel parent, ViewModelService modFileService)
        {
            _modFileService = modFileService;
            _modGroup = new()
            {
                GroupName = group.GroupName,
                SelectionType = group.SelectionType
            };
            foreach (var option in group.OptionList)
            {
                var optionViewModel = new ModOptionViewModel(option, this, modFileService);
                AddOption(optionViewModel);
            }
            RemoveCommand = new DelegateCommand(o => parent.RemoveGroup(this));
        }
        public void OnRemove()
        {
            RemoveCommand = null;
        }

        public string GroupName
        {
            get { return _modGroup.GroupName; }
            set { _modGroup.GroupName = value; OnPropertyChanged(); }
        }

        bool _singleSelection = true;
        public bool SingleSelection
        {
            get { return _singleSelection; }
            set
            {
                if (_singleSelection != value)
                {
                    _singleSelection = value;

                    SetSelectionType();
                    OnPropertyChanged();
                }
            }
        }

        string _newOptionName;
        public string NewOptionName
        {
            get { return _newOptionName; }
            set { _newOptionName = value; OnPropertyChanged(); }
        }

        DelegateCommand _addOptionCommand;
        public DelegateCommand AddOptionCommand
        {
            get { return _addOptionCommand ??= new DelegateCommand(o => AddOption()); }
        }

        public string SelectionType
        {
            get { return _modGroup.SelectionType; }
            set { _modGroup.SelectionType = value; OnPropertyChanged(); }
        }

        ObservableCollection<ModOptionViewModel> _optionList = new();
        public ObservableCollection<ModOptionViewModel> OptionList
        {
            get { return _optionList; }
            set { _optionList = value; OnPropertyChanged(); }
        }

        public DelegateCommand? RemoveCommand { get; set; }

        public int RemoveMods(IEnumerable<ModViewModel> mods)
        {
            var numRemoved = 0;
            foreach (var option in OptionList)
            {
                foreach (var mod in mods)
                {
                    if (option.RemoveMod(mod))
                    {
                        numRemoved++;
                    }
                }
                //numRemoved += option.RemoveMods(mods);
            }
            return numRemoved;
        }

        public ModGroup GetGroup()
        {
            return _modGroup;
        }

        private void SetSelectionType()
        {
            string sType;
            if (SingleSelection)
            {
                sType = "Single";
            }
            else
            {
                sType = "Multi";
            }

            SelectionType = sType;
        }

        public ModOptionViewModel? AddOption(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return null;
            }
            var modOption = new ModOption()
            {
                Name = str,
                SelectionType = "Single"
            };
            var optionViewModel = new ModOptionViewModel(modOption, this, _modFileService);
            AddOption(optionViewModel);
            return optionViewModel;
        }

        public void AddOption(ModOptionViewModel option)
        {
            _modGroup.OptionList.Add(option.GetModOption());
            OptionList.Add(option);
            //option._parent = this;
            NewOptionName = string.Empty;
            option.RemoveCommand = new DelegateCommand(o => RemoveOption(option));
        }

        public void RemoveOption(ModOptionViewModel option)
        {
            _modGroup.OptionList.Remove(option.GetModOption());
            OptionList.Remove(option);
            option.OnRemove();
        }

        private void AddOption()
        {
            AddOption(NewOptionName);
        }

        public List<ModOption> GetOptions()
        {
            return _modGroup.OptionList;
        }

        public bool IsEmpty()
        {
            return OptionList.Count == 0;
        }

        public void MoveTo(int oldIndex, int newIndex)
        {
            if (oldIndex < 0 || oldIndex > OptionList.Count)
            {
                return;
            }
            if (newIndex < 0 || newIndex > OptionList.Count)
            {
                return;
            }

            OptionList.Move(oldIndex, newIndex);
            _modGroup.OptionList.Move(oldIndex, newIndex);
        }

        public void MoveTo(ModOptionViewModel oldViewModel, ModOptionViewModel newViewModel)
        {
            var oldIndex = OptionList.IndexOf(oldViewModel);
            var newIndex = OptionList.IndexOf(newViewModel);

            MoveTo(oldIndex, newIndex);
        }

        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            var source = dropInfo.Data;
            var target = dropInfo.TargetItem;

            if (source is ModViewModel mod && target is ModOptionViewModel modOption)
            {
                if (modOption.CanAcceptMod(mod))
                {
                    dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                    dropInfo.Effects = DragDropEffects.Copy;
                }
            }
            else if (source is ModOptionViewModel sourceOption && target is ModOptionViewModel targetOption)
            {

            }
        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            // TODO: Multiple drag sources?
            var source = dropInfo.Data;
            var target = dropInfo.TargetItem;

            if (source is ModViewModel mod && target is ModOptionViewModel modOption)
            {
                if (modOption.CanAcceptMod(mod))
                {
                    modOption.AddMod(mod);
                }
            }
        }
    }
}