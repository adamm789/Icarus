using Icarus.Mods;
using Icarus.Services.Interfaces;
using Icarus.ViewModels.Util;
using ItemDatabase.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Windows.Data;
using xivModdingFramework.Materials.DataContainers;

namespace Icarus.ViewModels.Mods.Materials
{
    public class ShaderInfoViewModel : NotifyPropertyChanged
    {
        MaterialMod _materialMod;
        public IItem? SelectedItem;

        bool _canSetTexPaths = true;
        public bool CanSetTexPaths
        {
            get { return _canSetTexPaths; }
            set { _canSetTexPaths = value; OnPropertyChanged(); }
        }
        
        public ShaderInfoViewModel(MaterialMod material)
        {
            _materialMod = material;
            ShaderInfo = material.ShaderInfo;

            //NormalTex = new MaterialTexViewModel(_materialMod.NormalTexPath);

            NormalTexPath = _materialMod.NormalTexPath;
            SpecularTexPath = _materialMod.SpecularTexPath;
            MultiTexPath = _materialMod.MultiTexPath;
            DiffuseTexPath = _materialMod.DiffuseTexPath;
            ReflectionTexPath = _materialMod.ReflectionTexPath;

            Transparency = _materialMod.ShaderInfo.TransparencyEnabled ? enabled : disabled;
            TransparencyValues = new() { enabled, disabled };

            Backfaces = _materialMod.ShaderInfo.RenderBackfaces ? show : hide;
            BackfaceValues = new() { show, hide };

            if (_materialMod.IsFurniture)
            {
                ShadersList = new() { MtrlShader.Furniture };
            }
            else if (_materialMod.IsDyeableFurniture)
            {
                ShadersList = new() { MtrlShader.DyeableFurniture };
            }
            else
            {
                ShadersList = new()
                {
                    MtrlShader.Standard,
                    MtrlShader.Glass,
                    MtrlShader.Skin,
                    MtrlShader.Hair,
                    MtrlShader.Iris
                };
            }
        }

        protected void OnItemChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is MaterialModViewModel material)
            {
                SelectedItem = material.SelectedItem;
            }
        }

        public ShaderInfo ShaderInfo { get; }

        public void UpdatePaths(string str)
        {
            _materialMod.SetMtrlPath(str);

            OnPropertyChanged(nameof(NormalTexPath));
            OnPropertyChanged(nameof(MultiTexPath));
            OnPropertyChanged(nameof(DiffuseTexPath));
            OnPropertyChanged(nameof(SpecularTexPath));
            OnPropertyChanged(nameof(ReflectionTexPath));
        }

        public void ShaderInfoChanged()
        {
            OnPropertyChanged(nameof(CanSetTransparency));
            OnPropertyChanged(nameof(CanSetPreset));
            //CanSetTransparency = MtrlShader == MtrlShader.Standard;
            //CanSetPreset = (MtrlShader == MtrlShader.Standard || MtrlShader == MtrlShader.Skin || MtrlShader == MtrlShader.Hair);
            Preset = MtrlShaderPreset.Default;

            //NormalTexPath = _materialMod.NormalTexPath;
        }

        public void PresetChanged()
        {
            OnPropertyChanged(nameof(HasColorSet));
            OnPropertyChanged(nameof(HasMulti));
            OnPropertyChanged(nameof(HasDiffuse));
            OnPropertyChanged(nameof(HasSpecular));
            OnPropertyChanged(nameof(HasReflection));

            OnPropertyChanged(nameof(NormalTexPath));
            OnPropertyChanged(nameof(MultiTexPath));
            OnPropertyChanged(nameof(DiffuseTexPath));
            OnPropertyChanged(nameof(SpecularTexPath));
            OnPropertyChanged(nameof(ReflectionTexPath));
        }

        #region Bindings
        // TODO: CanSetShader = !furniture && !dyeablefurniture
        public bool CanSetShader
        {
            get { return MtrlShader != MtrlShader.Furniture && MtrlShader != MtrlShader.DyeableFurniture; }
        }

        public bool CanSetTransparency
        {
            get { return MtrlShader == MtrlShader.Standard; }
        }

        public bool CanSetPreset
        {
            get { return MtrlShader == MtrlShader.Standard || MtrlShader == MtrlShader.Skin || MtrlShader == MtrlShader.Hair; }
        }

        public MtrlShader MtrlShader
        {
            get { return _materialMod.ShaderInfo.Shader; }
            set
            {
                _materialMod.ShaderInfo.Shader = value;
                OnPropertyChanged();
                ShaderInfoChanged();
            }
        }

        public MtrlShaderPreset Preset
        {
            get { return _materialMod.ShaderInfo.Preset; }
            set
            {
                _materialMod.ShaderInfo.Preset = value;
                OnPropertyChanged();
                PresetChanged();
            }
        }

        // TODO: Shared and Unique textures?
        public bool UseSharedTextures = true;

        // TODO: HasColorSet and (dyeable) furniture?
        public bool HasColorSet => _materialMod.ShaderInfo.HasColorset;
        public bool HasMulti => _materialMod.ShaderInfo.HasMulti;
        public bool HasDiffuse => _materialMod.ShaderInfo.HasDiffuse;
        public bool HasSpecular => _materialMod.ShaderInfo.HasSpec;
        public bool HasReflection => _materialMod.ShaderInfo.HasReflection;

        public bool TransparencyEnabled
        {
            get { return _materialMod.ShaderInfo.TransparencyEnabled; }
            set { _materialMod.ShaderInfo.TransparencyEnabled = value; OnPropertyChanged(); }
        }

        public bool BackfacesEnabled
        {
            get { return _materialMod.ShaderInfo.RenderBackfaces; }
            set { _materialMod.ShaderInfo.RenderBackfaces = value; OnPropertyChanged(); }
        }

        // TODO: Look at TexTools tokenized texture paths
        public string NormalTexPath
        {
            get { return _materialMod.NormalTexPath; }
            set { _materialMod.NormalTexPath = value; OnPropertyChanged(); }
        }

        public string MultiTexPath
        {
            get { return _materialMod.MultiTexPath; }
            set { _materialMod.MultiTexPath = value; OnPropertyChanged(); }
        }

        public string SpecularTexPath
        {
            get { return _materialMod.SpecularTexPath; }
            set { _materialMod.SpecularTexPath = value; OnPropertyChanged(); }
        }

        public string DiffuseTexPath
        {
            get { return _materialMod.DiffuseTexPath; }
            set { _materialMod.DiffuseTexPath = value; OnPropertyChanged(); }
        }

        public string ReflectionTexPath
        {
            get { return _materialMod.ReflectionTexPath; }
            set { _materialMod.ReflectionTexPath = value; OnPropertyChanged(); }
        }

        List<MtrlShader> _shadersList;
        public List<MtrlShader> ShadersList
        {
            get { return _shadersList; }
            set { _shadersList = value; OnPropertyChanged(); }
        }

        List<MtrlShaderPreset> _shaderPresets = Enum.GetValues(typeof(MtrlShaderPreset)).Cast<MtrlShaderPreset>().ToList();

        ICollectionView _availableShaderPresets;
        public ICollectionView ShaderPresets
        {
            get
            {
                _availableShaderPresets = new CollectionViewSource { Source = _shaderPresets }.View;
                _availableShaderPresets.Filter = p => ShaderInfo.GetAvailablePresets(MtrlShader).Contains((MtrlShaderPreset)p);
                return _availableShaderPresets;
            }
        }

        // TODO: Better way to do transparency and show backfaces?

        public List<string> BackfaceValues { get; }
        const string show = "Show Backfaces";
        const string hide = "Hide Backfaces";

        string _backfaces;
        public string Backfaces
        {
            get { return _backfaces; }
            set
            {
                _backfaces = value;
                if (_backfaces == show)
                {
                    ShaderInfo.RenderBackfaces = true;
                }
                else if (_backfaces == hide)
                {
                    ShaderInfo.RenderBackfaces = false;
                }
            }
        }

        public List<string> TransparencyValues { get; }

        string enabled = "Enabled";
        string disabled = "Disabled";
        string _transparency;
        public string Transparency
        {
            get { return _transparency; }
            set
            {
                _transparency = value;
                if (value == enabled)
                {
                    ShaderInfo.TransparencyEnabled = true;
                }
                else if (value == disabled)
                {
                    ShaderInfo.TransparencyEnabled = false;
                }
            }
        }
        #endregion
    }
}
