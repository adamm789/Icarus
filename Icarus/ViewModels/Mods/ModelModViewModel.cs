using CommunityToolkit.Mvvm.DependencyInjection;
using ItemDatabase.Enums;
using Icarus.Mods;
using Icarus.Util;
using Icarus.ViewModels.Import;
using Icarus.ViewModels.Models;
using Lumina;
using Serilog;
using System;
using System.Collections.ObjectModel;
using xivModdingFramework.General.Enums;
using ItemDatabase.Interfaces;
using ItemDatabase;
using Icarus.Services.GameFiles;
using Icarus.Services;
using GongSolutions.Wpf.DragDrop;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using xivModdingFramework.Mods.DataContainers;
using xivModdingFramework.Models.DataContainers;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Icarus.Mods.Interfaces;
using Icarus.Mods.GameFiles;
using SharpDX.Direct2D1;
using xivModdingFramework.Models.FileTypes;
using ItemDatabase.Paths;
using Icarus.Services.GameFiles.Interfaces;

namespace Icarus.ViewModels.Mods
{
    // TODO: Figure out how to handle housing furniture...?
    // Housing furniture can have a letter at the very end of the file name
    // to indicate the part
    // e.g. bgcommon/hou/indoor/general/1142/bgparts/fun_b0_m1142a.mdl for the Steel Locker door
    // Will I have to distinguish between an "EquipmentModelModViewModel" and "FurnitureModelModViewModel"?
    public class ModelModViewModel : ModViewModel
    {
        public bool IsInternal => _modelMod.IsInternal;

        readonly ModelMod _modelMod;

        #region Constructors

        public ModelModViewModel(ModelMod modelMod, ViewModelService viewModelService, ItemListService itemListService, IGameFileService gameFileDataService)
            : base(modelMod, itemListService, gameFileDataService)
        {
            _modelMod = modelMod;
            var importedModel = modelMod.ImportedModel;

            if (importedModel != null)
            {
                foreach (var meshGroup in importedModel.MeshGroups)
                {
                    var meshGroupViewModel = viewModelService.GetMeshGroupViewModel(meshGroup, this);
                    MeshGroups.Add(meshGroupViewModel);
                }
            }

            OptionsViewModel = new(modelMod.Options);

            if (modelMod.IsComplete())
            {
                DisplayedHeader = $"{FileName} ({modelMod.Name})";

                //DestinationItem = modelMod.Item;
                if (HasSkin)
                {
                    TargetRace = modelMod.TargetRace;
                }
            }
            SetCanExport();
        }
        #endregion

        ModelModifierOptionsViewModel _optionsViewModel;
        public ModelModifierOptionsViewModel OptionsViewModel
        {
            get { return _optionsViewModel; }
            set { _optionsViewModel = value; OnPropertyChanged(); }
        }

        XivRace _targetRace = XivRace.Hyur_Midlander_Male;
        public XivRace TargetRace
        {
            get { return _targetRace; }
            set
            {
                _targetRace = value;
                OnPropertyChanged();
                UpdateTargetRace(_targetRace);
                _modelMod.TargetRace = _targetRace;
            }
        }

        public bool HasSkin
        {
            get { return XivPathParser.HasSkin(DestinationPath); }
        }

        ObservableCollection<MeshGroupViewModel> _meshGroups = new();
        public ObservableCollection<MeshGroupViewModel> MeshGroups
        {
            get { return _meshGroups; }
            set { _meshGroups = value; OnPropertyChanged(); }
        }

        public void UpdateTargetRace(XivRace race)
        {
            DestinationPath = XivPathParser.ChangeToRace(DestinationPath, race);
            OptionsViewModel.UpdateTargetRace(race);
        }

        /*
        public override Task SetDestinationItem(IItem? itemArg = null)
        {
            fromItem = true;
            var modData = _gameFileService.GetModelFileData(itemArg);
            if (modData != null)
            {
                SetModData(modData);
            }

            fromItem = false;
            return Task.CompletedTask;
        }
        */
        /*
        private void SetModData(IGameFile data)
        {
            if (data is ModelGameFile modelData)
            {
                var ttModel = modelData.TTModel;

                _mod.SetModData(modelData);
                UpdateAttributes(modelData);

                RaiseModPropertyChanged();
            }
        }
        */

        public override bool SetModData(IGameFile? gameFile)
        {
            if (gameFile is ModelGameFile modelGameFile)
            {
                UpdateAttributes(modelGameFile);
                return base.SetModData(modelGameFile);
            }
            return false;
        }

        // TODO: Double check: internal models should have their attributes set
        // TODO: Double check: internal models should always have their original attribute presets available
        private void UpdateAttributes(ModelGameFile modelData)
        {
            var ttModel = modelData.TTModel;
            var slot = XivPathParser.GetEquipmentSlot(modelData.Path);

            //var attributePresets = GetAttributePresets(modelData);
            //var slotAttributes = GetSlotAttributes(slot);
            var attributePresets = AttributePreset.GetAttributeTTModelPresets(ttModel);
            var bodyPresets = AttributePreset.GetAttributeBodyPresets(slot);
            foreach (var pair in bodyPresets)
            {
                attributePresets.TryAdd(pair.Key, pair.Value);
            }
            //var slotAttributes = AttributePreset.GetSlotAttributes(slot);
            var slotAttributes = AttributePreset.GetAttributes(modelData.Path);

            // TODO: Face attributes, hair attributes... other attributes?

            foreach (var group in MeshGroups)
            {
                group.SetAttributePresets(attributePresets);
                group.SetSlotAttributes(slotAttributes);
            }
        }
    }
}
