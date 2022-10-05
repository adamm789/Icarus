using Icarus.Mods;
using Icarus.ViewModels.Mods.Materials;
using Icarus.ViewModels.Util;
using ItemDatabase.Paths;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using xivModdingFramework.Materials.DataContainers;
using xivModdingFramework.Textures.Enums;

namespace Icarus.ViewModels.Mods
{
    public class ShaderInfoViewModel : NotifyPropertyChanged
    {

        MaterialMod _materialMod;

        public ShaderInfoViewModel(MaterialMod material)
        {
            ShaderInfo = material.ShaderInfo;
            _materialMod = material;

            //NormalTex = new MaterialTexViewModel(_materialMod.NormalTexPath);

            NormalTexPath = material.NormalTexPath;
            SpecularTexPath = material.SpecularTexPath;
            MultiTexPath = material.MultiTexPath;
            DiffuseTexPath = material.DiffuseTexPath;
            ReflectionTexPath = material.ReflectionTexPath;

            Transparency = _materialMod.ShaderInfo.TransparencyEnabled ? enabled : disabled;
            TransparencyValues = new() { enabled, disabled };

            Backfaces = _materialMod.ShaderInfo.RenderBackfaces ? show : hide;
            BackfaceValues = new() { show, hide };

            if (material.IsFurniture)
            {
                ShadersList = new() { MtrlShader.Furniture };
            }
            else if (material.IsDyeableFurniture)
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

        public void UpdatePaths(string str)
        {
            _materialMod.SetMtrlPath(str);
            ShaderInfoChanged();
        }

        // TODO: Prevent user from settings options that are mutually excelusive
        public ShaderInfo ShaderInfo { get; }
        public void ShaderInfoChanged()
        {
            OnPropertyChanged(nameof(HasColorSet));
            OnPropertyChanged(nameof(HasMulti));
            OnPropertyChanged(nameof(HasDiffuse));
            OnPropertyChanged(nameof(HasSpecular));
            OnPropertyChanged(nameof(HasReflection));

            OnPropertyChanged(nameof(CanSetTransparency));
            OnPropertyChanged(nameof(CanSetPreset));
            //CanSetTransparency = MtrlShader == MtrlShader.Standard;
            //CanSetPreset = (MtrlShader == MtrlShader.Standard || MtrlShader == MtrlShader.Skin || MtrlShader == MtrlShader.Hair);
            Preset = MtrlShaderPreset.Default;

            //NormalTexPath = _materialMod.NormalTexPath;
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
            get { return ShaderInfo.Shader; }
            set
            {
                ShaderInfo.Shader = value;
                OnPropertyChanged();
                ShaderInfoChanged();
            }
        }

        public MtrlShaderPreset Preset
        {
            get { return ShaderInfo.Preset; }
            set
            {
                ShaderInfo.Preset = value;
                OnPropertyChanged();
                //ShaderInfoChanged();
            }
        }

        // TODO: Shared and Unique textures?
        public bool UseSharedTextures = true;

        // TODO: HasColorSet and (dyeable) furniture?
        public bool HasColorSet => ShaderInfo.HasColorset;
        public bool HasMulti => ShaderInfo.HasMulti;
        public bool HasDiffuse => ShaderInfo.HasDiffuse;
        public bool HasSpecular => ShaderInfo.HasSpec;
        public bool HasReflection => ShaderInfo.HasReflection;

        public bool TransparencyEnabled
        {
            get { return ShaderInfo.TransparencyEnabled; }
            set { ShaderInfo.TransparencyEnabled = value; OnPropertyChanged(); }
        }

        public bool BackfacesEnabled
        {
            get { return ShaderInfo.RenderBackfaces; }
            set { ShaderInfo.RenderBackfaces = value; OnPropertyChanged(); }
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
                    Log.Debug("Transparency enabed");
                    ShaderInfo.TransparencyEnabled = true;
                }
                else if (value == disabled)
                {
                    Log.Debug("Transparency disabled");
                    ShaderInfo.TransparencyEnabled = false;
                }
            }
        }
        #endregion
    }
}
