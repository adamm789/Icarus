using Icarus.Services;
using Icarus.Services.GameFiles;
using Icarus.Services.Interfaces;
using Icarus.ViewModels.Mods;
using Icarus.ViewModels.Util;
using ItemDatabase.Interfaces;
using ItemDatabase.Paths;
using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using xivModdingFramework.General.Enums;
using xivModdingFramework.Models.DataContainers;

namespace Icarus.ViewModels.Models
{
    public class MeshGroupMaterialViewModel : NotifyPropertyChanged
    {
        // TODO: Consider when changing the race, it changes the material race as well
        // This probably results in materials that don't actually exist
        TTMeshGroup _importedGroup;
        ModelModViewModel _modelModViewModel;
        readonly IUserPreferencesService _userPreferencesService;

        private string _materialName = "";
        private string _materialVariant = "a";
        private string _skinName = "/mt_c0101b0001_";

        private string _initialMaterial = "";

        private string _skinVariant = "a";

        public MeshGroupMaterialViewModel(TTMeshGroup group, ModelModViewModel model, IUserPreferencesService userPreferences)
        {
            _importedGroup = group;
            _userPreferencesService = userPreferences;
            _modelModViewModel = model;
            _materialName = group.Material;

            _isSkinMaterial = XivPathParser.IsSkinMtrl(_importedGroup.Material);
            _initialMaterial = group.Material;
            DisplayedMaterial = _initialMaterial;
            DestinationModelPath = model.DestinationPath;

            UpdateRace(model.TargetRace);

            var eh = new PropertyChangedEventHandler(ParentChanged);
            model.PropertyChanged += eh;
        }

        private void ParentChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is not ModViewModel mod) return;

            if (e.PropertyName == nameof(mod.DestinationPath))
            {
                DestinationModelPath = mod.DestinationPath;
            }

            if (sender is not ModelModViewModel modelMod) return;

            if (e.PropertyName == nameof(modelMod.TargetRace))
            {
                //TargetRace = modelMod.TargetRace;
                UpdateRace(modelMod.TargetRace);
                UpdateDisplayedMaterial();
            }
        }

        string _destinationModelPath = "";
        public string DestinationModelPath
        {
            get { return _destinationModelPath; }
            set { _destinationModelPath = value; UpdateDisplayedMaterial(); }
        }

        bool _isSkinMaterial = false;
        public bool IsSkinMaterial
        {
            get { return _isSkinMaterial; }
            set
            {
                _isSkinMaterial = value;
                OnPropertyChanged();
                UpdateDisplayedMaterial();
            }
        }
        public bool CanAssignSkin => _modelModViewModel.HasSkin;


        // TODO: Implement an "exists" function, which keeps track of if a material exists or not
        // Whether the material is vanilla, or added as a mod
        // Different appearances?
        // TODO: Implement a "scroll to" that scrolls to the material if it is a mod?
        // But what if there are multiple materials that share the same name? Or how do I check "equality"?
        bool _exists = false;
        public bool Exists
        {
            get { return _exists; }
            set { _exists = value; OnPropertyChanged(); }
        }

        // TODO: Have material target race only affect the skin
        /*
        XivRace _targetRace = XivRace.Hyur_Midlander_Male;
        private XivRace TargetRace
        {
            get { return _targetRace; }
            set{_targetRace = value;
                //UpdateDisplayedMaterial();
            }
        }
        */

        public string DisplayedMaterial
        {
            get { return _importedGroup.Material; }
            set { _importedGroup.Material = value; OnPropertyChanged(); }
        }

        private void UpdateDisplayedMaterial()
        {
            if (XivPathParser.CanParsePath(DestinationModelPath))
            {
                UpdateItem();
                if (IsSkinMaterial)
                {
                    DisplayedMaterial = _skinName + _skinVariant + ".mtrl";
                }
                else
                {
                    DisplayedMaterial = _materialName;
                }
            }
        }

        private void UpdateRace(XivRace race)
        {
            _skinName = XivPathParser.ChangeToRace(_skinName, race);
            if (!_modelModViewModel.IsInternal)
            {
                _skinVariant = _userPreferencesService.GetDefaultSkinMaterialVariant(race);
            }
        }

        private void UpdateItem()
        {
            OnPropertyChanged(nameof(CanAssignSkin));
            _materialName = XivPathParser.GetMtrlFileName(DestinationModelPath);
            _materialName = XivPathParser.PathCorrectFileNameMtrl(_materialName);
        }
    }
}
