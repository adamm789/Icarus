using Icarus.Mods;
using Icarus.Mods.GameFiles;
using Icarus.Mods.Interfaces;
using Icarus.Services;
using Icarus.Services.GameFiles;
using Icarus.Services.GameFiles.Interfaces;
using Icarus.Services.Interfaces;
using Icarus.ViewModels.Import;
using Icarus.ViewModels.Models;
using Icarus.Util.Import;
using ItemDatabase;
using ItemDatabase.Enums;
using ItemDatabase.Interfaces;
using ItemDatabase.Paths;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using xivModdingFramework.General.Enums;
using xivModdingFramework.Models.Helpers;

namespace Icarus.ViewModels.Mods
{
    // TODO: Figure out how to handle housing furniture...?
    // Housing furniture can have a letter at the very end of the file name
    // to indicate the part
    // e.g. bgcommon/hou/indoor/general/1142/bgparts/fun_b0_m1142a.mdl for the Steel Locker door
    // Will I have to distinguish between an "EquipmentModelModViewModel" and "FurnitureModelModViewModel"?
    public class ModelModViewModel : ModViewModel
    {
        readonly ModelMod _modelMod;
        readonly IModelFileService _modelFileService;

        #region Constructors

        public ModelModViewModel(ModelMod modelMod, ViewModelService viewModelService, IModelFileService modelFileService, ILogService logService)
            : base(modelMod, modelFileService, logService)
        {
            _modelMod = modelMod;
            _modelFileService = modelFileService;

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
                //DisplayedHeader = $"{FileName} ({modelMod.Name})";

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

        public async override Task<IGameFile?> GetFileData(IItem? itemArg = null)
        {
            return await Task.Run(() => _modelFileService.GetModelFileData(itemArg, TargetRace));
        }

        public override bool SetModData(IGameFile? gameFile)
        {
            if (gameFile is ModelGameFile modelGameFile)
            {
                UpdateAttributes(modelGameFile);
                var ttModel = modelGameFile.TTModel;

                if (ttModel.MeshGroups.Count == MeshGroups.Count)
                {
                    for (var i = 0; i < ttModel.MeshGroups.Count; i++)
                    {
                        MeshGroups[i].MaterialViewModel.DisplayedMaterial = ttModel.MeshGroups[i].Material;
                    }
                }
                return base.SetModData(modelGameFile);
            }
            return false;
        }

        private void UpdateAttributes(ModelGameFile modelData)
        {
            // TODO: Include "variant" attributes
            var ttModel = modelData.TTModel;
            var slot = XivPathParser.GetEquipmentSlot(modelData.Path);

            var attributePresets = AttributePreset.GetAttributeTTModelPresets(ttModel);
            Dictionary<string, Dictionary<int, List<XivAttribute>>> internalPresets = new();
            if (ImportSource == ImportSource.Vanilla)
            {
                internalPresets = AttributePreset.GetAttributeTTModelPresets(_modelMod.ImportedModel);
            }

            var bodyPresets = AttributePreset.GetAttributeBodyPresets(slot);
            foreach (var pair in internalPresets)
            {
                attributePresets.TryAdd(pair.Key, pair.Value);
            }
            foreach (var pair in bodyPresets)
            {
                attributePresets.TryAdd(pair.Key, pair.Value);
            }

            var slotAttributes = AttributePreset.GetAttributes(modelData.Path);

            foreach (var group in MeshGroups)
            {
                group.SetAttributePresets(attributePresets);
                group.SetSlotAttributes(slotAttributes);
            }
        }

        protected override void RaiseDestinationPathChanged()
        {
            base.RaiseDestinationPathChanged();
            OnPropertyChanged(nameof(HasSkin));
        }
    }
}
