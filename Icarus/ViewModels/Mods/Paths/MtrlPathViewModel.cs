using Icarus.Mods.Interfaces;
using Icarus.Services.Interfaces;
using Icarus.Util.Import;
using Icarus.ViewModels.Util;
using ItemDatabase.Paths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using xivModdingFramework.General.Enums;

namespace Icarus.ViewModels.Mods.Paths
{
    public class MtrlPathViewModel : ViewModelBase
    {
        string _faceNumber = "f0001";
        string _skinRace = "c0101";
        string _bodyNumber = "b0001";
        string _materialName;


        ImportSource _importSource;
        IUserPreferencesService? _userPreferencesService;

        public MtrlPathViewModel(string material, IUserPreferencesService userPreferencesService, ImportSource importSource)
        {
            _importSource = importSource;
            _userPreferencesService = userPreferencesService;

            DisplayedMaterial = material;
            _materialName = material;

            var skinMatches = Regex.Match(DisplayedMaterial, @"(c[0-9]{4})");
            var skinVariantMatches = Regex.Match(DisplayedMaterial, @"c[0-9]{4}b[0-9]{4}_([a-z])");

            CanAssignSkin = skinMatches.Success;
            if (skinMatches.Success)
            {
                _skinRace = skinMatches.Groups[1].Value;
            }

            if (Regex.IsMatch(DisplayedMaterial, @"f[0-9]{4}"))
            {
                IsFaceMaterial = true;
                SetSelectedFaceMaterialIndex();
            }

            if (skinVariantMatches.Success)
            {
                IsSkinMaterial = true;
                SkinVariant = skinVariantMatches.Groups[1].Value;
            }
            else
            {
                MaterialVariant = XivPathParser.GetMtrlVariant(DisplayedMaterial);
            }
        }

        string _destinationModelPath = "";

        public void ChangeDestinationModel(IModelGameFile modelFile)
        {
            // TODO: Do it this way instead?
        }

        public void ChangeDestinationModel(string model)
        {
            _destinationModelPath = model;
            var skin = Regex.Match(_destinationModelPath, @"(c[0-9]{4})");
            CanAssignSkin = skin.Success;
            if (CanAssignSkin)
            {
                _skinRace = skin.Value;
            }
            SetFace();
            SetPath();
        }

        public bool ChangeRace(XivRace race)
        {
            _skinRace = XivPathParser.ChangeToRace(_skinRace, race);
            if (IsFaceMaterial)
            {
                DisplayedMaterial = $"/mt_{_skinRace}{_faceNumber}_{FaceMaterials[SelectedFaceMaterialIndex]}_{FaceVariant}.mtrl";
                OnPropertyChanged(nameof(DisplayedMaterial));
            }
            else if (IsSkinMaterial)
            {
                DisplayedMaterial = $"/mt_{_skinRace}{_bodyNumber}_{SkinVariant}.mtrl";
                OnPropertyChanged(nameof(DisplayedMaterial));
            }
            return true;
        }

        private void SetFace()
        {
            var faceMatch = Regex.Match(_destinationModelPath, @"(f[0-9]{4})");
            IsFaceMaterial = faceMatch.Success;
            if (IsFaceMaterial)
            {
                _faceNumber = faceMatch.Groups[1].Value;
            }
            SetSelectedFaceMaterialIndex();
        }

        private void SetSelectedFaceMaterialIndex()
        {
            CanParseFaceMaterial = IsFaceMaterial;
            if (Regex.IsMatch(DisplayedMaterial, @"fac"))
            {
                SelectedFaceMaterialIndex = 0;
            }
            else if (Regex.IsMatch(DisplayedMaterial, @"_iri"))
            {
                SelectedFaceMaterialIndex = 1;
            }
            else if (Regex.IsMatch(DisplayedMaterial, @"_etc"))
            {
                SelectedFaceMaterialIndex = 2;
            }
            else
            {
                CanParseFaceMaterial = false;
            }
        }

        private void SetPath()
        {
            // TODO: Furniture material shenanigans
            // TODO?: Read material from the model file instead?

            if (XivPathParser.CanParsePath(_destinationModelPath))
            {
                _materialName = XivPathParser.GetMtrlFileName(_destinationModelPath, _materialVariant);
                _materialName = XivPathParser.PathCorrectFileNameMtrl(_materialName);
            }

            var bodyMatch = Regex.Match(_destinationModelPath, @"(b[0-9]{4})");
            if (bodyMatch.Success)
            {
                _bodyNumber = bodyMatch.Groups[1].Value;
            }

            if (_importSource == ImportSource.Raw)
            {
                var race = XivPathParser.GetRaceFromString(_skinRace);
                var variant = _userPreferencesService?.GetDefaultSkinMaterialVariant(race);
                if (variant != null)
                {
                    SkinVariant = variant;
                }
            }

            if (IsFaceMaterial)
            {
                DisplayedMaterial = $"/mt_{_skinRace}{_faceNumber}_{FaceMaterials[SelectedFaceMaterialIndex]}_{FaceVariant}.mtrl";
                SetSelectedFaceMaterialIndex();
            }
            else if (IsSkinMaterial)
            {
                DisplayedMaterial = $"/mt_{_skinRace}{_bodyNumber}_{SkinVariant}.mtrl";
            }
            else
            {
                DisplayedMaterial = _materialName;
            }
            OnPropertyChanged(nameof(DisplayedMaterial));
        }


        bool _canParseFaceMaterial;
        public bool CanParseFaceMaterial
        {
            get { return _canParseFaceMaterial; }
            set { _canParseFaceMaterial = value; OnPropertyChanged(); }
        }


        List<string> _faceMaterials = new()
        {
            "fac",
            "iri",
            "etc"
        };
        public List<string> FaceMaterials
        {
            get { return _faceMaterials; }
        }

        int _selectedFaceMaterialIndex = 0;
        public int SelectedFaceMaterialIndex
        {
            get { return _selectedFaceMaterialIndex; }
            set
            {
                _selectedFaceMaterialIndex = value;
                OnPropertyChanged();
                var x = Regex.IsMatch(DisplayedMaterial, @"[iri|fac|etc]");
                _displayedMaterial = Regex.Replace(DisplayedMaterial, @"(fac|iri|etc)", FaceMaterials[_selectedFaceMaterialIndex]);
                OnPropertyChanged(nameof(DisplayedMaterial));
            }
        }

        string _displayedMaterial;
        public string DisplayedMaterial
        {
            get { return _displayedMaterial; }
            set
            {
                _displayedMaterial = value;
                ChangeDisplayedMaterial();
            }
        }

        string _materialVariant = "a";
        public string MaterialVariant
        {
            get { return _materialVariant; }
            set
            {
                _materialVariant = value;
                ChangeMaterialVariant(_materialVariant);
            }
        }

        private void ChangeDisplayedMaterial()
        {
            try
            {
                var variant = XivPathParser.GetMtrlVariant(DisplayedMaterial);

                if (IsSkinMaterial)
                {
                    _skinVariant = variant;
                    OnPropertyChanged(nameof(SkinVariant));
                }
                else if (IsFaceMaterial)
                {
                    _faceVariant = variant;
                    OnPropertyChanged(nameof(FaceVariant));
                    SetSelectedFaceMaterialIndex();
                }
                else
                {
                    _materialVariant = variant;
                    OnPropertyChanged(nameof(MaterialVariant));
                }
            }
            catch (Exception ex)
            {
                _logService?.Error($"Could not get material variant from: {DisplayedMaterial}");
                _logService?.Debug(ex);
            }
        }

        public void ChangeMaterialVariant(string variant)
        {
            if (IsFaceMaterial)
            {
                _faceVariant = variant;
            }
            else if (IsSkinMaterial)
            {
                _skinVariant = variant;
            }
            else
            {
                _materialVariant = variant;
            }
            _displayedMaterial = XivPathParser.ChangeMtrlVariant(DisplayedMaterial, variant);
            OnPropertyChanged(nameof(DisplayedMaterial));
        }

        string _skinVariant = "a";
        public string SkinVariant
        {
            get { return _skinVariant; }
            set
            {
                _skinVariant = value;
                OnPropertyChanged();
                ChangeMaterialVariant(value);
            }
        }

        string _faceVariant = "a";
        public string FaceVariant
        {
            get { return _faceVariant; }
            set { _faceVariant = value; OnPropertyChanged(); }
        }

        bool _isFaceMaterial = false;
        public bool IsFaceMaterial
        {
            get { return _isFaceMaterial; }
            set
            {
                _isFaceMaterial = value;
                OnPropertyChanged();
                CanAssignSkin = CanAssignSkin && !_isFaceMaterial;
            }
        }

        bool _isSkinMaterial = false;
        public bool IsSkinMaterial
        {
            get { return _isSkinMaterial; }
            set
            {
                _isSkinMaterial = value;
                OnPropertyChanged();
                SetPath();
            }
        }

        bool _canAssignSkin = false;
        public bool CanAssignSkin
        {
            get { return _canAssignSkin; }
            set { _canAssignSkin = value; OnPropertyChanged(); }
        }
    }
}
