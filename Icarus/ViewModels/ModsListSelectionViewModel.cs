using Icarus.Services.Interfaces;
using Icarus.ViewModels.Mods;
using Icarus.ViewModels.Mods.DataContainers;
using Icarus.ViewModels.Mods.DataContainers.Interfaces;
using Icarus.ViewModels.Util;
using Icarus.Views.Mods;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.ViewModels
{
    public abstract class ModsListSelectionViewModel : UIViewModelBase
    {
        protected IModsListViewModel _modsListViewModel;
        public FilteredModsListViewModel FilteredMods { get; set; }
        protected PropertyChangedEventHandler _eh;
        protected Type _selectedType = typeof(ModViewModel);
        protected string _modType = "";

        public ModsListSelectionViewModel(IModsListViewModel modsListViewModel, ILogService logService) : base(logService)
        {
            _modsListViewModel = modsListViewModel;
            FilteredMods = new(_modsListViewModel, logService);
            OnPropertyChanged(nameof(FilteredMods));
            _eh = new(OnModsListPropertyChanged);

            foreach (var m in _modsListViewModel.SimpleModsList)
            {
                m.PropertyChanged += _eh;
            }

            FilteredMods.PropertyChanged += new(OnFilteredModsListPropertyChanged);
            _modsListViewModel.SimpleModsList.CollectionChanged += new(OnCollectionChanged);

            UpdateText();
        }

        bool _shouldApplyToAll = true;
        public bool ShouldApplyToAll
        {
            get { return _shouldApplyToAll; }
            set {
                _shouldApplyToAll = value;
                OnPropertyChanged();
                foreach (var m in _modsListViewModel.SimpleModsList)
                {
                    if (_selectedType.IsInstanceOfType(m))
                    {
                        Apply(m, value);
                    }
                }
            }
        }

        string _applyToAllText;
        public string ApplyToAllText
        {
            get { return _applyToAllText; }
            set { _applyToAllText = value; OnPropertyChanged(); }
        }

        protected string _confirmText = "";
        public string ConfirmText
        {
            get { return _confirmText; }
            set { _confirmText = value; OnPropertyChanged(); }
        }

        protected abstract void Apply(ModViewModel mvm, bool value);
        protected abstract void InvertMod(ModViewModel mvm);
        protected abstract void UpdateText();

        protected void UpdateText(int numSelected, int totalTypeSelected)
        {
            if (numSelected != totalTypeSelected)
            {
                _shouldApplyToAll = false;
                ApplyToAllText = $"Select All {_modType}";
            }
            else
            {
                _shouldApplyToAll = true;
                ApplyToAllText = $"Unselect All {_modType}";
            }
            OnPropertyChanged(nameof(ShouldApplyToAll));
        }

        protected abstract void OnModsListPropertyChanged(object sender, PropertyChangedEventArgs e);

        private void OnFilteredModsListPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is FilteredModsListViewModel modsList && e.PropertyName == nameof(FilteredModsListViewModel.SelectedType))
            {
                _selectedType = modsList.SelectedType;
                if (modsList.SelectedType == typeof(ModelModViewModel))
                {
                    _modType = "Models";
                }
                else if (modsList.SelectedType == typeof(MaterialModViewModel))
                {
                    _modType = "Materials";
                }
                else if (modsList.SelectedType == typeof(TextureModViewModel))
                {
                    _modType = "Textures";
                }
                else if (modsList.SelectedType == typeof(MetadataModViewModel))
                {
                    _modType = "Metadata";
                }
                else if (modsList.SelectedType == typeof(ReadOnlyModViewModel))
                {
                    _modType = "Metadata";
                }
                else
                {
                    _modType = "";
                }
                UpdateText();
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                var mods = e.NewItems.Cast<ModViewModel>();
                foreach (var m in mods)
                {
                    m.PropertyChanged += _eh;
                }
                UpdateText();
            }
            if (e.OldItems != null)
            {
                UpdateText();
            }
        }

        DelegateCommand _onConfirmCommand;
        public virtual DelegateCommand OnConfirmCommand
        {
            get { return _onConfirmCommand ??= new DelegateCommand(_ => ConfirmCommand()); }
        }

        public abstract void ConfirmCommand();

        DelegateCommand _onCancelCommand;
        public virtual DelegateCommand OnCancelCommand
        {
            get { return _onCancelCommand ??= new DelegateCommand(_ => CancelCommand()); }
        }
        protected abstract void CancelCommand();

        DelegateCommand _invertSelectionCommand;
        public DelegateCommand InvertSelectionCommand
        {
            get { return _invertSelectionCommand ??= new DelegateCommand(_ => InvertSelection()); }
        }

        private void InvertSelection()
        {
            foreach (var m in _modsListViewModel.SimpleModsList)
            {
                if (_selectedType.IsInstanceOfType(m))
                {
                    InvertMod(m);
                }
            }
        }
    }
}
