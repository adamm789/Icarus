using Icarus.Mods.DataContainers;
using Icarus.Services;
using Icarus.ViewModels.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Icarus.ViewModels.Mods.DataContainers
{
    public class ModPackPageViewModel : NotifyPropertyChanged
    {
        protected ModPackPage _modPackPage;
        readonly ViewModelService _modFileService;

        public ModPackPageViewModel(int index, ModPackViewModel parent, ViewModelService modFileService)
        {
            _modPackPage = new(index + 1);
            _modFileService = modFileService;
            RemoveCommand = new(o => parent.RemovePage(this));
        }

        public ModPackPageViewModel(ModPackPage page, ModPackViewModel parent, ViewModelService modFileService)
        {
            _modPackPage = new(page);
            _modFileService = modFileService;
            foreach (var group in page.ModGroups)
            {
                //var groupViewModel = new ModGroupViewModel(group, gameFileDataService, windowService);
                var groupViewModel = new ModGroupViewModel(group, this, _modFileService);
                AddGroup(groupViewModel);
            }
            RemoveCommand = new(o => parent.RemovePage(this));
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
            set
            {
                _modPackPage.PageIndex = value;
                OnPropertyChanged();
            }
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

        ObservableCollection<ModGroupViewModel> _modGroups = new();
        public ObservableCollection<ModGroupViewModel> ModGroups
        {
            get { return _modGroups; }
            set { _modGroups = value; OnPropertyChanged(); }
        }

        public ModGroupViewModel AddGroup(string groupName)
        {
            var vm = new ModGroupViewModel(groupName, this, _modFileService);
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
                var vm = new ModGroupViewModel(NewGroupName, this, _modFileService);

                AddGroup(vm);
                NewGroupName = string.Empty;
            }
        }

        public ModPackPage GetModPackPage()
        {
            return _modPackPage;
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

        public DelegateCommand? RemoveCommand { get; set; }
    }
}
