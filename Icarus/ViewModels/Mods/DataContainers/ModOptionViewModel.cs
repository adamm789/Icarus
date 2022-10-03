using GongSolutions.Wpf.DragDrop;
using Icarus.Mods.DataContainers;
using Icarus.Mods.Interfaces;
using Icarus.Services;
using Icarus.Services.GameFiles;
using Icarus.Services.Interfaces;
using Icarus.ViewModels.Util;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Icarus.ViewModels.Mods.DataContainers
{
    // TODO: Highlight mod in simplemodslist on left when hovered over?
    public class ModOptionViewModel : NotifyPropertyChanged, IDropTarget
    {
        protected ModOption _modOption = new();
        readonly ViewModelService _modFileService;
        ModGroupViewModel _parent;

        PropertyChangedEventHandler eh;

        public ModOptionViewModel(ModOption option, ModGroupViewModel parent, ViewModelService modFileService)
        {
            _modOption = new ModOption(option);
            _modFileService = modFileService;
            _parent = parent;

            RemoveCommand = new(o => _parent.RemoveOption(this));

            foreach (var mod in option.Mods)
            {
                var modViewModel = _modFileService.GetModViewModel(mod);
                AddMod(modViewModel);
            }
            eh = new(OnSelectionTypeChanged);
            _parent.PropertyChanged += eh;
            UpdateHeader();
        }

        #region Bindings

        string _header = "";
        public string Header
        {
            get { return _header; }
            set { _header = value; OnPropertyChanged(); }
        }
        public string Name
        {
            get { return _modOption.Name; }
            set { _modOption.Name = value; OnPropertyChanged(); }
        }

        public string ImagePath
        {
            get { return _modOption.ImagePath; }
            set { _modOption.ImagePath = value; }
        }

        public string GroupName
        {
            get { return _modOption.GroupName; }
            set { _modOption.GroupName = value; }
        }

        public bool IsChecked
        {
            get { return _modOption.IsChecked; }
            set { _modOption.IsChecked = value; }
        }

        public string Description
        {
            get { return _modOption.Description; }
            set { _modOption.Description = value; OnPropertyChanged(); }
        }

        public string SelectionType
        {
            get { return _modOption.SelectionType; }
            set { _modOption.SelectionType = value; }
        }

        ObservableCollection<ModOptionModViewModel> _modViewModels = new();
        public ObservableCollection<ModOptionModViewModel> ModViewModels
        {
            get { return _modViewModels; }
            set { _modViewModels = value; OnPropertyChanged(); }
        }

        public DelegateCommand? RemoveCommand { get; set; }

        #endregion
        public void AddMod(ModViewModel vm)
        {
            var modOptionMod = new ModOptionModViewModel(this, vm);
            AddMod(modOptionMod);
        }

        public void AddMod(IMod mod)
        {
            var vm = _modFileService.GetModViewModel(mod);
            AddMod(vm);
        }

        public void AddMod(ModOptionModViewModel mod)
        {
            ModViewModels.Add(mod);
            _modOption.Mods.Add(mod.ModViewModel.GetMod());
            UpdateHeader();
        }

        public List<IMod> GetMods()
        {
            return _modOption.Mods;
        }

        public ModOption GetModOption()
        {
            return _modOption;
        }

        public void OnRemove()
        {
            _parent.PropertyChanged -= eh;
        }

        public void RemoveMod(IMod mod)
        {
            var m = ModViewModels.FirstOrDefault(o => o.ModViewModel.GetMod() == mod);
            if (m != null)
            {
                RemoveMod(m);
            }
        }

        // TODO: Should I delete the option/group/page if there are no more mods?
        public bool RemoveMod(ModViewModel mod)
        {
            var m = ModViewModels.FirstOrDefault(p => p.ModViewModel.GetMod() == mod.GetMod());
            if (m != null)
            {
                return RemoveMod(m);
            }
            return false;
        }

        public bool RemoveMod(ModOptionModViewModel mod)
        {
            Log.Verbose($"Removing {mod.GetMod().Name} from {Name}.");
            var b0 = ModViewModels.Remove(mod);
            var b1 = _modOption.Mods.Remove(mod.GetMod());
            UpdateHeader();
            return b0 && b1;
        }

        public int RemoveMods(IEnumerable<IMod> mods)
        {
            var startSize = ModViewModels.Count;
            foreach (var mod in mods)
            {
                foreach (var modOptionMod in ModViewModels)
                {
                    if (mod == modOptionMod.GetMod())
                    {
                        RemoveMod(mod);
                    }
                }
            }
            return ModViewModels.Count - startSize;
        }

        public int RemoveMods(IEnumerable<ModViewModel> mods)
        {
            return RemoveMods(mods.Select(m => m.GetMod()));
        }

        public int RemoveMods(IEnumerable<ModOptionModViewModel> mods)
        {
            return RemoveMods(mods.Select(m => m.ModViewModel));
        }

        public bool CanAcceptMod(ModViewModel mod)
        {
            return mod != null && !ModExists(mod);
        }

        private bool ModExists(ModViewModel mod)
        {
            foreach (var mm in ModViewModels)
            {
                if (mm.ModViewModel.GetMod() == mod.GetMod())
                {
                    return true;
                }
            }
            return false;
        }

        private void OnSelectionTypeChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ModGroupViewModel.SelectionType))
            {
                var parent = sender as ModGroupViewModel;
                SelectionType = parent.SelectionType;
            }
        }

        private void UpdateHeader()
        {
            Header = $"Mods ({ModViewModels.Count})";
        }

        #region UI

        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            var dragItem = dropInfo.Data;
            // TODO: Allow multi-select/multi-drop
            // TODO: Allow scrolling and/or moving the list while dragging

            if (dragItem is ModViewModel modViewModel)
            {
                if (CanAcceptMod(modViewModel))
                {
                    dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                    dropInfo.Effects = DragDropEffects.Copy;
                }
            }
            else if (dragItem is ModOptionViewModel)
            {
                // TODO: Different effects for when they share the same group parent (to change ordering within groups)
                // or different group parent (to move between groups)
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Copy;
            }
        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            var dropItem = dropInfo.Data;
            if (dropItem is ModViewModel modViewModel)
            {
                if (CanAcceptMod(modViewModel))
                {
                    AddMod(modViewModel);
                }
            }
            else if (dropItem is ModOptionViewModel modOption)
            {
                if (_parent == modOption._parent)
                {
                    _parent.MoveTo(modOption, this);
                }
                else
                {
                    modOption._parent.RemoveOption(modOption);
                    _parent.AddOption(modOption);
                }
            }
        }
        #endregion
    }
}
