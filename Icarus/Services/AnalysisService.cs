using Icarus.Services.GameFiles;
using Icarus.Services.Interfaces;
using Icarus.ViewModels.Models;
using Icarus.ViewModels.Mods;
using Icarus.ViewModels.Mods.DataContainers.Interfaces;
using Icarus.ViewModels.Mods.Materials;
using Icarus.ViewModels.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.Services
{
    //TODO: Class that analysis and assesses "incomplete" mods or "missing" paths/files
    public class AnalysisService : LuminaDependentServiceBase<AnalysisService>
    {
        private ObservableCollection<ModViewModel> _modsList;
        public AnalysisService(ObservableCollection<ModViewModel> modsList, LuminaService luminaService)
            : base(luminaService)
        {
            _modsList = modsList;
            _modsList.CollectionChanged += new(OnCollectionChanged);
        }

        protected override void OnLuminaSet()
        {

        }

        // Used to determine if a mtrl or tex exists in the mod list (or is vanilla)
        // Lots of cases...
        // New material, new texture, new model
        // DestinationPath change in material, texture, and model
        // Preset change in material.ShaderInfo
        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count > 0)
            {
                foreach (var item in e.NewItems)
                {
                    ProcessNewMods(item);
                }
            }
            if (e.OldItems != null && e.OldItems.Count > 0)
            {
                foreach (var item in e.OldItems)
                {

                }
            }
        }

        private void ProcessNewMods(object? mod)
        {
            if (mod is MaterialModViewModel mtrlMod)
            {
                // add mtrlMod to dictionary
                // go through dictionary to see if mtrlMod.DestinationPath is present
                // update mtrlMod.Exists appropriately

                var shaderInfo = mtrlMod.ShaderInfoViewModel;
                var eh = new PropertyChangedEventHandler(OnMaterialChanged);
                if (!eventHandlers.ContainsKey(mtrlMod))
                {
                    eventHandlers[mtrlMod] = new List<PropertyChangedEventHandler>();
                }
                eventHandlers[mtrlMod].Add(eh);
                shaderInfo.PropertyChanged += eh;
            }
            else if (mod is TextureModViewModel texMod)
            {
                // add texture
            }
            else if (mod is ModelModViewModel mdlMod)
            {
                // add each unique mesh group material

                foreach (var meshGroup in mdlMod.MeshGroups)
                {
                    var eh = new PropertyChangedEventHandler(OnModelMeshGroupChanged);
                    if (!eventHandlers.ContainsKey(mdlMod))
                    {
                        eventHandlers[mdlMod] = new List<PropertyChangedEventHandler>();
                    }
                    eventHandlers[mdlMod].Add(eh);
                    var meshGroupMaterial = meshGroup.MaterialViewModel;
                    meshGroupMaterial.PropertyChanged += eh;
                }
            }
        }

        private Dictionary<ModViewModel, IList<PropertyChangedEventHandler>> eventHandlers = new();

        private void OnModelMeshGroupChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is MeshGroupMaterialViewModel meshGroupMaterial && e.PropertyName == nameof(MeshGroupMaterialViewModel.DisplayedMaterial))
            {
                // Check to see if material path is vanilla
                // Then check in list of modded materials
            }
        }

        private void OnMaterialChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is ShaderInfoViewModel shaderInfo)
            {
                // for each tex type...
                // update in-use texture
                // re-check to see if any modded textures' material has changed
                if (e.PropertyName == nameof(ShaderInfoViewModel.NormalTexPath))
                {

                }
            }
        }

        private void OnTextureChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is TextureModViewModel textureMod && e.PropertyName == nameof(TextureModViewModel.DestinationPath))
            {
                // Check if texture is vanilla
                var path = textureMod.DestinationPath;
                if (_lumina.FileExists(path))
                {
                    // TODO: How to express "completeness"
                }

                // Loop through modded materials and see if this texture path is referenced
            }
        }
    }
}
