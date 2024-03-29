﻿using Icarus.Services;
using Icarus.Services.Interfaces;
using Icarus.ViewModels.Mods;
using Icarus.ViewModels.Util;
using Icarus.Views.Models;
using ItemDatabase;
using ItemDatabase.Enums;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using xivModdingFramework.Models.DataContainers;

namespace Icarus.ViewModels.Models
{
    public class MeshGroupViewModel : NotifyPropertyChanged
    {
        public string Name { get; set; }
        private TTMeshGroup _importedGroup;
        readonly IWindowService _windowService;

        public MeshGroupViewModel(TTMeshGroup group, ModelModViewModel modelMod, ViewModelService viewModelService, IWindowService windowService)
        {
            if (AllAttributes == null)
            {
                AllAttributes = new();
                foreach (var attr in AttributePreset.GetAllAttributes())
                {
                    if (attr.IsVariantAttribute())
                    {
                        var attrViewModel = new VariantAttributeViewModel(attr);
                        AllAttributes.Add(attrViewModel);
                    }
                    else
                    {
                        var attrViewModel = new AttributeViewModel(attr);
                        AllAttributes.Add(attrViewModel);
                    }
                }
            }

            MaterialViewModel = viewModelService.GetMeshGroupMaterialViewModel(group, modelMod);
            _importedGroup = group;
            Name = group.Name;
            _windowService = windowService;

            var numAttributes = 0;
            foreach (var meshPart in _importedGroup.Parts)
            {
                var meshPartViewModel = new MeshPartViewModel(meshPart);
                meshPartViewModel.Attributes.CollectionChanged += new(OnAttributesCollectionChanged);
                MeshParts.Add(meshPartViewModel);
                numAttributes += meshPartViewModel.NumAttributes;
                foreach (var key in meshPart.ShapeParts.Keys)
                {
                    if (!ShapesContains(key))
                    {
                        var shapeViewModel = new ShapeViewModel(key);
                        var eh = new PropertyChangedEventHandler(OnRemoveShape);
                        shapeViewModel.PropertyChanged += eh;
                        Shapes.Add(shapeViewModel);
                    }
                }
            }
            NumAttributes = numAttributes;
        }

        public int NumShapes => Shapes.Count;

        private void OnAttributesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var numAttributes = 0;
            foreach (var mp in MeshParts)
            {
                numAttributes += mp.NumAttributes;
            }
            NumAttributes = numAttributes;
        }

        int _numAttributes;
        public int NumAttributes
        {
            get { return _numAttributes; }
            set { _numAttributes = value; OnPropertyChanged(); }
        }

        ObservableCollection<AttributePresetsViewModel> _attributePresets = new();
        public ObservableCollection<AttributePresetsViewModel> AttributePresets
        {
            get { return _attributePresets; }
            set { _attributePresets = value; OnPropertyChanged(); }
        }

        ObservableCollection<AttributeViewModel> _slotAttributes = new();
        public ObservableCollection<AttributeViewModel> SlotAttributes
        {
            get { return _slotAttributes; }
            set { _slotAttributes = value; OnPropertyChanged(); }
        }

        public static List<AttributeViewModel> AllAttributes { get; set; }

        public void SetAttributePresets(Dictionary<string, Dictionary<int, List<string>>>? bodyPresets)
        {
            AttributePresets.Clear();

            if (bodyPresets != null)
            {
                foreach (var key in bodyPresets.Keys)
                {
                    var presets = new AttributePresetsViewModel(key, bodyPresets[key]);

                    // TODO: CanCopyPreset -> CanExecute in this DelegateCommand?
                    presets.CopyPresetCommand = new DelegateCommand(o => CopyPreset(presets));
                    AttributePresets.Add(presets);
                }
            }
        }

        public void SetSlotAttributes(ObservableCollection<AttributeViewModel> slotAttributes)
        {
            SlotAttributes.Clear();
            SlotAttributes = slotAttributes;
        }

        // TODO: Handle "variant" attributes (e.g. atr_tv_a, atr_tv_b, etc...)
        public void SetSlotAttributes(List<XivAttribute>? slotAttributes)
        {
            SlotAttributes.Clear();
            if (slotAttributes != null)
            {
                foreach (var xivAttr in slotAttributes)
                {
                    var attr = new AttributeViewModel(xivAttr);
                    SlotAttributes.Add(attr);
                }
            }
        }

        private void CopyPreset(AttributePresetsViewModel preset)
        {
            var dict = preset.Dictionary;
            foreach (var kvp in dict)
            {
                var key = kvp.Key;
                if (key < MeshParts.Count)
                {
                    MeshParts[key].AddAttributes(kvp.Value);
                }
            }
        }

        private void OnRemoveShape(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ShapeViewModel.ShouldRemove) && sender is ShapeViewModel shape)
            {
                Shapes.Remove(shape);
                foreach (var meshPart in _importedGroup.Parts)
                {
                    meshPart.ShapeParts.Remove(shape.Name);
                }
                OnPropertyChanged(nameof(NumShapes));
            }
        }

        private bool ShapesContains(string s)
        {
            foreach (var shape in Shapes)
            {
                if (shape.Name == s)
                {
                    return true;
                }
            }
            return false;
        }

        ObservableCollection<ShapeViewModel> _shapes = new();
        public ObservableCollection<ShapeViewModel> Shapes
        {
            get { return _shapes; }
            set { _shapes = value; OnPropertyChanged(); }
        }

        ObservableCollection<MeshPartViewModel> _meshParts = new();
        public ObservableCollection<MeshPartViewModel> MeshParts
        {
            get { return _meshParts; }
            set { _meshParts = value; OnPropertyChanged(); }
        }

        MeshGroupMaterialViewModel _materialViewModel;
        public MeshGroupMaterialViewModel MaterialViewModel
        {
            get { return _materialViewModel; }
            set { _materialViewModel = value; OnPropertyChanged(); }
        }

        DelegateCommand _editShapeAndAttributesCommand;
        public DelegateCommand EditShapeAndAttributesCommand
        {
            get { return _editShapeAndAttributesCommand ??= new DelegateCommand(o => EditShapeAndAttributes()); }
        }

        public void EditShapeAndAttributes()
        {
            _windowService.ShowWindow<ShapeAndAttributeWindow>(this);
        }
    }
}
