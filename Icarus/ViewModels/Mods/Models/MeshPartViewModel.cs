using GongSolutions.Wpf.DragDrop;
using Icarus.ViewModels.Util;
using ItemDatabase;
using ItemDatabase.Enums;
using Serilog;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using xivModdingFramework.Models.DataContainers;

namespace Icarus.ViewModels.Models
{
    public class MeshPartViewModel : NotifyPropertyChanged, IDropTarget
    {
        public TTMeshPart MeshPart;

        public MeshPartViewModel(TTMeshPart part)
        {
            MeshPart = part;

            foreach (var atr in MeshPart.Attributes)
            {
                AddAttribute(atr);
            }
            Name = MeshPart.Name;
        }

        #region Bindings

        string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged(); }
        }

        public int NumAttributes
        {
            get { return Attributes.Count; }
        }

        string _attributeTextBox;
        public string AttributeTextBox
        {
            get { return _attributeTextBox; }
            set { _attributeTextBox = value; OnPropertyChanged(); }
        }

        ObservableCollection<AttributeViewModel> _attributes = new();
        public ObservableCollection<AttributeViewModel> Attributes
        {
            get { return _attributes; }
        }

        DelegateCommand _addAttributeCommand;
        public DelegateCommand AddAttributeCommand
        {
            get { return _addAttributeCommand ??= new DelegateCommand(o => AddAttribute()); }
        }

        #endregion

        /// <summary>
        /// Tries to add the <paramref name="str"/> to the attributes list.
        /// </summary>
        /// <param name="str"></param>
        public void AddAttribute(string str)
        {
            if (!string.IsNullOrWhiteSpace(str) && IndexOfAttribute(str) == -1)
            {
                var attribute = new AttributeViewModel(str);
                AddAttribute(attribute);
            }
        }

        public void AddAttribute(AttributeViewModel attr)
        {
            if (IndexOfAttribute(attr.GetAttributeString()) != -1)
            {
                return;
            }
            var attribute = attr.Copy();
            var eh = new PropertyChangedEventHandler(OnRemoveAttribute);
            attribute.PropertyChanged += eh;
            Attributes.Add(attribute);
            AttributeTextBox = "";

            // TODO: Error checking for correct attributes?
            MeshPart.Attributes.Add(attribute.GetAttributeString());
            OnPropertyChanged(nameof(NumAttributes));
        }

        public void AddAttributes(IEnumerable<string> strings)
        {
            foreach (var attr in strings)
            {
                AddAttribute(attr);
            }
        }

        public void AddAttributes(IEnumerable<XivAttribute> attributes)
        {
            foreach (var attr in attributes)
            {
                AddAttribute(XivAttributes.GetStringFromAttribute(attr));
            }
        }

        public void OnRemoveAttribute(object sender, PropertyChangedEventArgs e)
        {
            var attribute = sender as AttributeViewModel;

            if (e.PropertyName == nameof(AttributeViewModel.ShouldRemove))
            {
                Attributes.Remove(attribute);
                MeshPart.Attributes.Remove(attribute.GetAttributeString());
                OnPropertyChanged(nameof(NumAttributes));
            }
        }

        public void RemoveAttribute(string str)
        {
            var idx = IndexOfAttribute(str);
            if (idx != -1)
            {
                var vm = Attributes[idx];
                vm.RemoveCommand.Execute(vm);
            }
        }

        /// <summary>
        /// Gets the string from the text box
        /// </summary>
        private void AddAttribute()
        {
            AddAttribute(AttributeTextBox);
        }

        /// <summary>
        /// Searches for the AttributeViewModel which holds the string <paramref name="str"/>.
        /// </summary>
        /// <param name="str"></param>
        /// <returns>The zero-based index of the first occurrence within Attributes, if found; otherwise, -1.</returns>
        private int IndexOfAttribute(string str)
        {
            foreach (var vm in Attributes)
            {
                if (vm.GetAttributeString() == str)
                {
                    return Attributes.IndexOf(vm);
                }
            }
            return -1;
        }

        private bool CanAcceptChild(object? o)
        {
            if (o is AttributeViewModel attr)
            {
                return IndexOfAttribute(attr.GetAttributeString()) == -1;
            }
            else if (o is AttributePresetsViewModel attrPreset)
            {
                return true;
            }
            else if (o is PartAttributesViewModel part)
            {
                foreach (var partAttribute in part.Attributes)
                {
                    if (IndexOfAttribute(partAttribute.GetAttributeString()) == -1)
                    {
                        return true;
                    }
                }
                return false;
                // TODO: Allow "partial addition"?
                // i.e. when a part already has one of the attributes, this would just add the missing attributes
            }
            return false;
        }

        public void DragOver(IDropInfo dropInfo)
        {
            var dragItem = dropInfo.Data;
            if (CanAcceptChild(dragItem))
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Copy;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            Log.Debug($"Drop onto {GetType()}");
            var dropItem = dropInfo.Data;
            if (dropItem is AttributeViewModel attr)
            {
                AddAttribute(attr);
            }
            else if (dropItem is PartAttributesViewModel part)
            {
                foreach (var partAttribute in part.Attributes)
                {
                    AddAttribute(partAttribute);
                }
            }
            else if (dropItem is AttributePresetsViewModel preset)
            {
                preset.CopyPresetCommand?.Execute(preset);
            }
        }
    }
}
