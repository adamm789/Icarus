using Icarus.Mods;
using Icarus.Mods.Interfaces;
using Icarus.Services.Interfaces;
using Icarus.Util.Import;
using Icarus.ViewModels.Mods;
using Icarus.ViewModels.Util;
using ItemDatabase.Paths;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using xivModdingFramework.General.Enums;
using xivModdingFramework.Models.DataContainers;

namespace Icarus.ViewModels.Models
{
    public class MeshGroupMaterialViewModel : NotifyPropertyChanged
    {
        TTMeshGroup _importedGroup;
        ModelModViewModel _modelModViewModel;
        readonly IUserPreferencesService _userPreferencesService;

        private string _materialName = "";
        private string _skinName = "/mt_c0101b0001_";
        private string _initialMaterial = "";

        public MeshGroupMaterialViewModel(TTMeshGroup group, ModelModViewModel model, IUserPreferencesService userPreferences)
        {
            _importedGroup = group;
            _userPreferencesService = userPreferences;
            _modelModViewModel = model;

            _materialName = group.Material;
            _isSkinMaterial = XivPathParser.IsSkinMtrl(_importedGroup.Material);
            _initialMaterial = group.Material;
            DisplayedMaterial = _initialMaterial;
            MaterialVariant = XivPathParser.GetMtrlVariant(DisplayedMaterial);
            DestinationModelPath = model.DestinationPath;

            InitializeSkinVariant(model.TargetRace);

            //UpdateRace(model.TargetRace);

            PropertyChanged += new PropertyChangedEventHandler(OnMyPropertyChanged);

            var eh = new PropertyChangedEventHandler(ParentChanged);
            model.PropertyChanged += eh;
        }


        string _skinVariant = "a";
        public string SkinVariant
        {
            get { return _skinVariant; }
            set { _skinVariant = value; OnPropertyChanged(); }
        }

        string _materialVariant = "a";
        public string MaterialVariant
        {
            get { return _materialVariant; }
            set { _materialVariant = value; OnPropertyChanged(); }
        }

        public bool CanParsePath => XivPathParser.CanParsePath(DestinationModelPath);
        public bool CanAssignSkin { get => _modelModViewModel.HasSkin; }

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

        private void OnMyPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SkinVariant) || e.PropertyName == nameof(MaterialVariant)
                || e.PropertyName == nameof(DestinationModelPath) || e.PropertyName == nameof(IsSkinMaterial))
            {
                UpdateDisplayedMaterial();
            }

            if (e.PropertyName == nameof(DisplayedMaterial))
            {
                UpdateVariants();
            }
        }

        string _destinationModelPath = "";
        public string DestinationModelPath
        {
            get { return _destinationModelPath; }
            set { _destinationModelPath = value; OnPropertyChanged(); }
        }

        bool _isSkinMaterial = false;
        public bool IsSkinMaterial
        {
            get { return _isSkinMaterial; }
            set { _isSkinMaterial = value; OnPropertyChanged(); }
        }

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
            set
            {
                _importedGroup.Material = value; OnPropertyChanged();
                // TODO: Fix up changing race and manually changing displayed material
                /*
                if (_importedGroup.Material == value) return;

                _importedGroup.Material = value;
                if (XivPathParser.IsSkinMtrl(value))
                {
                    _isSkinMaterial = true;
                    //OnPropertyChanged(nameof(IsSkinMaterial));
                }

                OnPropertyChanged();
                if (IsSkinMaterial)
                {
                    SkinVariant = XivPathParser.GetMtrlVariant(value);
                }
                else
                {
                    MaterialVariant = XivPathParser.GetMtrlVariant(value);
                }
                */
            }
        }

        private void UpdateVariants()
        {
            if (IsSkinMaterial)
            {
                _skinVariant = XivPathParser.GetMtrlVariant(DisplayedMaterial);
            }
            else
            {
                _materialVariant = XivPathParser.GetMtrlVariant(DisplayedMaterial);
            }
        }

        private void UpdateDisplayedMaterial()
        {
            OnPropertyChanged(nameof(CanParsePath));
            if (CanParsePath)
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
            // TODO: UserPreferences of default skin material will overwrite any assignment in, for example, a ttmp2

            if (_modelModViewModel.ImportSource == ImportSource.Raw)
            {
                SkinVariant = _userPreferencesService.GetDefaultSkinMaterialVariant(race);
            }
            else if (_modelModViewModel.ImportSource == ImportSource.TexToolsModPack)
            {
                if (IsSkinMaterial)
                {
                    SkinVariant = XivPathParser.GetMtrlVariant(DisplayedMaterial);
                }
                else
                {
                    SkinVariant = _userPreferencesService.GetDefaultSkinMaterialVariant(race);
                }
            }
        }

        private void InitializeSkinVariant(XivRace race)
        {
            if (_modelModViewModel.ImportSource == ImportSource.Raw)
            {
                _skinVariant = _userPreferencesService.GetDefaultSkinMaterialVariant(race);
            }
            else if (_modelModViewModel.ImportSource == ImportSource.TexToolsModPack)
            {
                if (IsSkinMaterial)
                {
                    _skinVariant = XivPathParser.GetMtrlVariant(DisplayedMaterial);
                }
                else
                {
                    _skinVariant = _userPreferencesService.GetDefaultSkinMaterialVariant(race);
                }
            }
        }

        private void UpdateItem()
        {
            OnPropertyChanged(nameof(CanAssignSkin));
            _materialName = XivPathParser.GetMtrlFileName(DestinationModelPath, _materialVariant);
            _materialName = XivPathParser.PathCorrectFileNameMtrl(_materialName);
        }

        public List<string> VariantList { get; } = new()
        {
            "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m",
            "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z"
        };
    }
}
