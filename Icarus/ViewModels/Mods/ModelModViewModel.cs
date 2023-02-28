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
using System.Configuration;
using xivModdingFramework.Models.DataContainers;
using System.IO;

namespace Icarus.ViewModels.Mods
{
    // TODO: When assigning (at least a hair) the materials do not keep their variants

    // TODO: Figure out how to handle housing furniture...?
    // Housing furniture can have a letter at the very end of the file name
    // to indicate the part
    // e.g. bgcommon/hou/indoor/general/1142/bgparts/fun_b0_m1142a.mdl for the Steel Locker door
    // Will I have to distinguish between an "EquipmentModelModViewModel" and "FurnitureModelModViewModel"?

    // TODO: "Does not have a valid skin element" i.e. no armature (?)
    // More forceful warning if no armature? because that can cause a crash

    // TODO: Changing the target race changes the destination name to the selected item
    public class ModelModViewModel : ModViewModel
    {
        readonly ModelMod _modelMod;
        readonly IModelFileService _modelFileService;

        public IModelGameFile ModelFile
        {
            get; protected set;
        }

        #region Constructors

        public ModelModViewModel(ModelMod modelMod, ViewModelService viewModelService, IModelFileService modelFileService, ILogService logService)
            : base(modelMod, modelFileService, logService)
        {
            _modelMod = modelMod;
            _modelFileService = modelFileService;

            var importedModel = modelMod.ImportedModel;

            OptionsViewModel = new(modelMod.Options);
            if (modelMod.IsComplete())
            {
                if (HasSkin)
                {
                    TargetRace = modelMod.TargetRace;
                }
            }

            if (importedModel != null)
            {
                foreach (var meshGroup in importedModel.MeshGroups)
                {
                    var meshGroupViewModel = viewModelService.GetMeshGroupViewModel(meshGroup, this);
                    MeshGroups.Add(meshGroupViewModel);
                }
            }

            if (modelMod.IsComplete())
            {
                UpdateAttributes(modelMod.TTModel, modelMod.Path);
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
            _modelMod.Path = XivPathParser.ChangeToRace(DestinationPath, race);
            OnPropertyChanged(nameof(DestinationPath));
            OptionsViewModel.UpdateTargetRace(race);
        }

        public async override Task<IGameFile?> GetFileData(IItem? itemArg = null)
        {
            return await Task.Run(() => _modelFileService.GetModelFileData(itemArg, TargetRace));
        }

        public async override Task<IGameFile?> GetFileData(string path, string name = "")
        {
            return await Task.Run(() =>_modelFileService.TryGetModelFileData(path, name));
        }

        public override bool SetModData(IGameFile? gameFile)
        {
            if (gameFile is IModelGameFile modelGameFile)
            {
                ModelFile = modelGameFile;
                var ttModel = modelGameFile.TTModel;
                UpdateAttributes(ttModel, modelGameFile.Path);

                // TODO: I think assigning the ImportedModel.Source here is safe?
                _modelMod.ImportedModel.Source = ttModel.Source;
                if (ttModel.MeshGroups.Count == MeshGroups.Count)
                {
                    for (var i = 0; i < ttModel.MeshGroups.Count; i++)
                    {
                        MeshGroups[i].MaterialViewModel.DisplayedMaterial = ttModel.MeshGroups[i].Material;
                    }
                }
                else if (ttModel.MeshGroups.Count == 1)
                {
                    foreach (var group in MeshGroups)
                    {
                        group.MaterialViewModel.DisplayedMaterial = ttModel.MeshGroups[0].Material;
                    }
                }
                // TODO: What to do if current model has more or fewer materials and neither has only one material?
                return base.SetModData(modelGameFile);
            }
            return false;
        }

        // TODO: Gear that covers multiple slots, e.g. Chocobo Suit, should attributes from ALL of its covered slots
        private void UpdateAttributes(TTModel ttModel, string path)
        {
            var slot = XivPathParser.GetEquipmentSlot(path);

            var attributePresets = AttributePreset.GetAttributeTTModelPresets(ttModel);
            Dictionary<string, Dictionary<int, List<string>>> internalPresets = new();
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
                var stringDict = new Dictionary<int, List<string>>();
                foreach (var partPresets in pair.Value)
                {
                    stringDict.Add(partPresets.Key, partPresets.Value.ConvertAll<string>(attr => XivAttributes.GetStringFromAttribute(attr)));
                }
                attributePresets.TryAdd(pair.Key, stringDict);
            }

            var attributeList = AttributePreset.GetAttributes(path);

            var slotAttributes = new ObservableCollection<AttributeViewModel>();
            foreach (var attr in attributeList)
            {
                if (attr.IsVariantAttribute())
                {
                    slotAttributes.Add(new VariantAttributeViewModel(attr));
                }
                else
                {
                    slotAttributes.Add(new AttributeViewModel(attr));
                }
            }

            if (MeshGroups.Count == 0)
            {
                _logService?.Warning($"This model has no mesh groups: {_modelMod.Name}");
            }

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

        protected override bool HasValidPathExtension(string path)
        {
            return Path.GetExtension(path) == ".mdl";
        }
    }
}
