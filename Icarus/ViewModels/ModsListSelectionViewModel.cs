using Icarus.Services.Interfaces;
using Icarus.ViewModels.Mods;
using Icarus.ViewModels.Mods.DataContainers;
using Icarus.ViewModels.Mods.DataContainers.Interfaces;
using Icarus.ViewModels.Util;
using Icarus.Views.Mods;
using Newtonsoft.Json.Linq;
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

            _modsListViewModel.SimpleModsList.CollectionChanged += new(OnCollectionChanged);
            UpdateText();

            UpdateAllText();
        }

        // TODO: "AreModels" to toggle visibility of the option to select all?

        bool _allModels = true;
        public bool AllModels
        {
            get { return _allModels; }
            set
            {
                _allModels = value;
                OnPropertyChanged();
                ApplyToType(typeof(ModelModViewModel), value);
                UpdateText(ref _allModelsText, typeof(ModelModViewModel), value);
                OnPropertyChanged(nameof(AllModelsText));
            }
        }

        string _allModelsText = "All Model Mods";
        public string AllModelsText
        {
            get { return _allModelsText; }
            set { _allModelsText = value; OnPropertyChanged(); }
        }

        bool _allMaterials = true;
        public bool AllMaterials
        {
            get { return _allMaterials; }
            set
            {
                _allMaterials = value;
                OnPropertyChanged();
                ApplyToType(typeof(MaterialModViewModel), value);
                UpdateText(ref _allMaterialsText, typeof(MaterialModViewModel), value);
                OnPropertyChanged(nameof(AllMaterialsText));
            }
        }

        bool _allTextures = true;
        public bool AllTextures
        {
            get { return _allTextures; }
            set
            {
                _allTextures = value;
                OnPropertyChanged();
                ApplyToType(typeof(TextureModViewModel), value);
                UpdateText(ref _allTexturesText, typeof(TextureModViewModel), value);
                OnPropertyChanged(nameof(AllTexturesText));

            }
        }

        string _allTexturesText = "All Texture Mods";
        public string AllTexturesText
        {
            get { return _allTexturesText; }
            set { _allTexturesText = value; OnPropertyChanged(); }
        }

        string _allMaterialsText = "All Material Mods";
        public string AllMaterialsText
        {
            get { return _allMaterialsText; }
            set { _allMaterialsText = value; OnPropertyChanged(); }
        }

        bool _allMetadata = true;
        public bool AllMetadata
        {
            get { return _allMetadata; }
            set
            {
                _allMetadata = value;
                OnPropertyChanged();
                ApplyToType(typeof(MetadataModViewModel), value);
                UpdateText(ref _allMetadataText, typeof(MetadataModViewModel), value);
                OnPropertyChanged(nameof(AllMetadataText));
            }
        }

        string _allMetadataText = "All Metadata Mods";
        public string AllMetadataText
        {
            get { return _allMetadataText; }
            set { _allMetadataText = value; OnPropertyChanged(); }
        }

        bool _allReadOnly = true;
        public bool AllReadOnly
        {
            get { return _allReadOnly; }
            set
            {
                _allReadOnly = value;
                OnPropertyChanged();
                ApplyToType(typeof(ReadOnlyModViewModel), value);
                UpdateText(ref _allReadOnlyText, typeof(ReadOnlyModViewModel), value);
                OnPropertyChanged(nameof(AllReadOnlyText));
            }
        }

        string _allReadOnlyText = "All ReadOnly Mods";
        public string AllReadOnlyText
        {
            get { return _allReadOnlyText; }
            set { _allReadOnlyText = value; OnPropertyChanged(); }
        }

        protected void UpdateAllText()
        {
            _allModels = FilteredMods.ModelMods.AllSelected;
            _allMaterials = FilteredMods.MaterialMods.AllSelected;
            _allTextures = FilteredMods.TextureMods.AllSelected;
            _allMetadata = FilteredMods.MetadataMods.AllSelected;
            _allReadOnly = FilteredMods.ReadOnlyMods.AllSelected;

            OnPropertyChanged(nameof(AllModels));
            OnPropertyChanged(nameof(AllMaterials));
            OnPropertyChanged(nameof(AllTextures));
            OnPropertyChanged(nameof(AllMetadata));
            OnPropertyChanged(nameof(AllMetadata));
        }

        protected string _confirmText = "";
        public string ConfirmText
        {
            get { return _confirmText; }
            set { _confirmText = value; OnPropertyChanged(); }
        }

        protected abstract void Apply(ModViewModel mvm, bool value);
        protected void ApplyToType(Type type, bool value)
        {
            var list = _modsListViewModel.SimpleModsList.Where(x => type.IsInstanceOfType(x));
            foreach (var val in list)
            {
                Apply(val, value);
            }
        }
        protected abstract void InvertMod(ModViewModel mvm);
        protected abstract void UpdateText();

        protected virtual void UpdateText(ref string text, Type t, bool allSelected = true)
        {
            var typeString = "";
            if (t == typeof(ModelModViewModel))
            {
                typeString = "Model";
            }
            else if (t == typeof(MaterialModViewModel))
            {
                typeString = "Material";
            }
            else if (t == typeof(TextureModViewModel))
            {
                typeString = "Texture";
            }
            else if (t == typeof(MetadataModViewModel))
            {
                typeString = "Metadata";
            }
            else if (t == typeof(ReadOnlyModViewModel))
            {
                typeString = "ReadOnly";
            }
            var verbage = "Select";
            if (allSelected)
            {
                verbage = "Unselect";
            }
            text = $"All {typeString} mods";
        }

        protected abstract void OnModsListPropertyChanged(object sender, PropertyChangedEventArgs e);

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
