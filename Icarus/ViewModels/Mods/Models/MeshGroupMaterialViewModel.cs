using Icarus.Mods;
using Icarus.Mods.Interfaces;
using Icarus.Services.Interfaces;
using Icarus.Util.Import;
using Icarus.ViewModels.Mods;
using Icarus.ViewModels.Mods.Paths;
using Icarus.ViewModels.Util;
using ItemDatabase.Paths;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Text.RegularExpressions;
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

        public MtrlPathViewModel MtrlPathViewModel
        {
            get; set;
        }

        public MeshGroupMaterialViewModel(TTMeshGroup group, ModelModViewModel model, IUserPreferencesService userPreferences)
        {
            _importedGroup = group;
            _userPreferencesService = userPreferences;
            _modelModViewModel = model;

            if (string.IsNullOrWhiteSpace(group.Material))
            {
                group.Material = "";
            }

            MtrlPathViewModel = new(ref group.Material, userPreferences, model.ImportSource);
            MtrlPathViewModel.ChangeRace(model.TargetRace);
            MtrlPathViewModel.PropertyChanged += new PropertyChangedEventHandler(OnMaterialChanged);

            model.PropertyChanged += new PropertyChangedEventHandler(OnModelChanged);
        }

        private void OnMaterialChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MtrlPathViewModel.DisplayedMaterial))
            {
                DisplayedMaterial = MtrlPathViewModel.DisplayedMaterial;
            }
        }

        private void OnModelChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ModelModViewModel.DestinationPath))
            {
                MtrlPathViewModel.ChangeDestinationModel(_modelModViewModel.DestinationPath);
            }
            if (e.PropertyName == nameof(ModelModViewModel.TargetRace))
            {
                MtrlPathViewModel.ChangeRace(_modelModViewModel.TargetRace);
            }
        }


        public string SkinVariant
        {
            get { return MtrlPathViewModel.SkinVariant; }
            set { MtrlPathViewModel.SkinVariant = value; OnPropertyChanged(); }
        }

        public string MaterialVariant
        {
            get { return MtrlPathViewModel.MaterialVariant; }
            set { MtrlPathViewModel.MaterialVariant = value; OnPropertyChanged(); }
        }

        public bool IsSkinMaterial
        {
            get { return MtrlPathViewModel.IsSkinMaterial; }
            set { MtrlPathViewModel.IsSkinMaterial = value; OnPropertyChanged(); }
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

        // TODO: How to handle assignment to "Character" models, e.g. faces which have multiple different materials?
        public string DisplayedMaterial
        {
            get { return _importedGroup.Material; }
            set { _importedGroup.Material = value; OnPropertyChanged(); }
        }
    }
}
